using DocumentService.PolyglotPersistence.DBContext;
using DocumentService.PolyglotPersistence.IRepositories;
using DocumentService.PolyglotPersistence.IServices;
using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.PermissionModels;
using DocumentService.PolyglotPersistence.Models.ResponseModels;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DocumentService.PolyglotPersistence.Repositories
{
    public class MongoRepository : IRepository
    {
        private readonly MongoDBContext _mongoDBContext;
        private readonly IAzureBlobServices _azureBlobService;
        private readonly IConfiguration _configuration;
        private readonly CloudBlobContainer _documentContentContainer;
        private readonly CloudBlobContainer _historyContentContainer;

        private string AzureBlobState => _configuration.GetSection("AzureBlobStorage").GetSection("Actived").Value;
        private string AzureDocumentContainerName => _configuration.GetSection("AzureBlobStorage").GetSection("DocumentContainer").Value;
        private string AzureHistoryContainerName => _configuration.GetSection("AzureBlobStorage").GetSection("HistoryContainer").Value;

        public MongoRepository(MongoDBContext mongoDBContext, IAzureBlobServices azureBlobService, IConfiguration configuration)
        {
            _mongoDBContext = mongoDBContext;
            _configuration = configuration;

            _azureBlobService = azureBlobService;
            _documentContentContainer = _azureBlobService.ConnectToAzureBlobStorage(AzureDocumentContainerName);
            _historyContentContainer = _azureBlobService.ConnectToAzureBlobStorage(AzureHistoryContainerName);

        }

        #region // document services
        // get document data
        public Document? GetDocument(Guid documentId) =>
            _mongoDBContext.document.Find(x => x._id == documentId).SingleOrDefault();

        // get document by path_url
        public bool CheckDocumentName(Guid parentFolderId, string name) =>
            _mongoDBContext.document.Find(x => x.folder_id == parentFolderId && x.document_name == name).Any();
            
        // create new document and its content
        public void CreateDocument(Document document) =>
            _mongoDBContext.document.InsertOne(document);

        // update document, save previous version in history
        [Obsolete("Guid representation")]
        public void CreateDocumentVersion(DocumentHistory documentVersion)
        {
            // insert older version of document into history
            _mongoDBContext.document_history.InsertOne(documentVersion);

            // copy content from document content to history content
            if (_mongoDBContext.document_content.Find(x => x._id == documentVersion.document_id).Any())
            {
                var aggDoc = new BsonDocument
                {
                    {"aggregate" , "document_content"},
                    {"pipeline", new BsonArray
                        {
                            new BsonDocument {
                                { "$match" , new BsonDocument { {"_id", documentVersion.document_id} } }
                            },
                            new BsonDocument {
                                { "$group" , new BsonDocument{
                                        {"_id", documentVersion._id},
                                        {"document_id", new BsonDocument{{"$first", "$_id"}}},
                                        {"content", new BsonDocument{{"$first", "$content"}}},
                                        {"content_type", new BsonDocument{{"$first", "$content_type"}}}
                                    }
                                }
                            },
                            new BsonDocument {
                                { "$merge" , "document_history_content" }
                            }
                        }
                    },
                    {"cursor", BsonDocument.Parse("{}") }
                };

                BsonDocumentCommand<BsonDocument> command = new(aggDoc);
                _mongoDBContext.mongoDatabase.RunCommand(command);
            }
            // if document is not located in mongoDB then copy from AzureBlob
            else
                _azureBlobService.CopyBlobContent(_documentContentContainer, _historyContentContainer, documentVersion.document_id.ToString());
        }

        // rename document
        public void RenameDocument(Document document, string name)
        {
            FilterDefinition<Document> docTBU = Builders<Document>.Filter.Eq(d => d._id, document._id);
            _mongoDBContext.document.ReplaceOne(docTBU, document);
        }

        // delete document
        public void DeleteDocument(Document document, bool deleteHistory)
        {
            // remove document and its content
            FilterDefinition<Document> documentTBD = Builders<Document>.Filter.Eq(d => d._id, document._id);
            _mongoDBContext.document.DeleteOne(documentTBD);
            FilterDefinition<DocumentContent> documentContentTBD = Builders<DocumentContent>.Filter.Eq(d => d._id, document._id);
            _mongoDBContext.document_content.DeleteOne(documentContentTBD);

            // azure document blob container
            _azureBlobService.DeleteBlob(_documentContentContainer, document._id.ToString());

            // removes everything related to document if required
            if (deleteHistory)
            {
                // remove versions info and content of document
                FilterDefinition<DocumentHistory> documentHistoryTBD = Builders<DocumentHistory>.Filter.Eq(d => d.document_id, document._id);
                _mongoDBContext.document_history.DeleteMany(documentHistoryTBD);
                FilterDefinition<DocumentHistoryContent> documentHistoryContentTBD = Builders<DocumentHistoryContent>.Filter.Eq(d => d.document_id, document._id);
                _mongoDBContext.document_history_content.DeleteMany(documentHistoryContentTBD);

                // check if there is document on AzureBlob and remove it
                _azureBlobService.DeleteBlob(_historyContentContainer, document._id.ToString());
            }
        }

        [Obsolete("Guid representation")]
        public void RestoreDocument(Document document)
        {
            // find latest version in history and add it to document table
            DocumentHistory documentVersion = _mongoDBContext.document_history.Find(dh => dh.document_id == document._id && dh.version == (document.version - 1)).Single();
            _mongoDBContext.document.InsertOne(document);

            // copy content from history document content to document content
            if (_mongoDBContext.document_history_content.Find(x => x._id == documentVersion._id).Any())
            {
                var aggDoc = new BsonDocument
                {
                    {"aggregate" , "document_history_content"},
                    {"pipeline", new BsonArray
                        {
                            new BsonDocument {
                                { "$match" , new BsonDocument { {"_id", documentVersion._id } } }
                            },
                            new BsonDocument {
                                { "$group" , new BsonDocument{
                                        {"_id", documentVersion.document_id},
                                        {"content", new BsonDocument{{"$first", "$content"}}},
                                        {"content_type", new BsonDocument{{"$first", "$content_type"}}}
                                    }
                                }
                            },
                            new BsonDocument {
                                { "$merge" , "document_content" }
                            }
                        }
                    },
                    {"cursor", BsonDocument.Parse("{}") }
                };

                BsonDocumentCommand<BsonDocument> command = new(aggDoc);
                _mongoDBContext.mongoDatabase.RunCommand(command);

                FilterDefinition<DocumentHistoryContent> documentHistoryContentTBD = Builders<DocumentHistoryContent>.Filter.Eq(d => d._id, documentVersion._id);
                _mongoDBContext.document_history_content.DeleteOne(documentHistoryContentTBD);
            }
            // if document is not located in mongoDB then copy from AzureBlob
            else
            {
                _azureBlobService.CopyBlobContent(_historyContentContainer, _documentContentContainer, documentVersion._id.ToString());
                _azureBlobService.DeleteBlob(_historyContentContainer, documentVersion._id.ToString());
            }
            // remove versions info and content of document
            FilterDefinition<DocumentHistory> documentHistoryTBD = Builders<DocumentHistory>.Filter.Eq(d => d._id, documentVersion._id);
            _mongoDBContext.document_history.DeleteOne(documentHistoryTBD);
        }
        #endregion

        #region // document content services
        // get document content
        public byte[] GetDocumentContentInChunks(Guid? documentId, int offset, int chunk_size)
        {
            // if document is not located in document_content, get it from Azure Blob Storage
            if (!_mongoDBContext.document_content.Find(x => x._id == documentId).Any())
            {
                byte[] buffer = new byte[chunk_size];
                var cloudClient = _documentContentContainer.GetBlobReference(documentId.ToString());
                cloudClient.DownloadRangeToByteArray(buffer, 0, offset, chunk_size);

                return buffer;
            }

            return _mongoDBContext.document_content.Find(x => x._id == documentId).Single().content;
        }

        // create document content
        public void CreateDocumentContent(DocumentContent documentContent) =>
            _mongoDBContext.document_content.InsertOne(documentContent);

        // create document content in Azure Blob
        public int CreateDocumentContentInChunks(Guid documentId, byte[] buffer, ref HashSet<string> blocklist)
        {
            // if stream is empty, return 0 as sign to stop uploading/updating
            if (buffer.Length == 0)
            {
                // post blocklist
                _azureBlobService.PutBlocklist(_documentContentContainer, documentId.ToString(), blocklist);

                // remove content from database 
                FilterDefinition<DocumentContent> documentContent = Builders<DocumentContent>.Filter.Eq(d => d._id, documentId);
                _mongoDBContext.document_content.DeleteOne(documentContent);
                return 0;
            }

            // upload to Azure Blob
            blocklist = _azureBlobService.UploadToAzureBlobStorage(_documentContentContainer, documentId.ToString(), buffer, blocklist);

            // return buffer size
            return buffer.Length;
        }
        #endregion

        #region // history services
        // get all document versions from history
        public IEnumerable<DocumentHistory> GetDocumentHistory(Guid? documentId) =>
            _mongoDBContext.document_history.Find(x => x.document_id == documentId).ToEnumerable();

        // returns content of requested document version
        public DocumentHistory? GetDocumentByVersionHistory(Guid? documentId, int version) =>
            _mongoDBContext.document_history.Find(x => x.document_id == documentId && x.version == version).SingleOrDefault();

        // return buffer of history content
        public byte[] GetDocumentContentVersionInChunks(Guid? documentId, int offset, int chunk_size)
        {
            // if document is not located in document_content, get it from AzureBlob storage
            if (!_mongoDBContext.document_history_content.Find(x => x._id == documentId).Any())
            {
                byte[] buffer = new byte[chunk_size];
                var cloudClient = _historyContentContainer.GetBlobReference(documentId.ToString());
                cloudClient.DownloadRangeToByteArray(buffer, 0, offset, chunk_size);

                return buffer;
            }

            return _mongoDBContext.document_history_content.Find(x => x._id == documentId).Single().content;
        }
        #endregion

        #region // check in/out services
        // does checkin and returns updated document if successful
        public void CheckInDocument(Document document)
        {
            FilterDefinition<Document> docTBU = Builders<Document>.Filter.Eq(d => d._id, document._id);
            _mongoDBContext.document.ReplaceOne(docTBU, document);
        }

        // does checkout and returns updated document if successful
        public void CheckOutDocument(Document document, string requestorId)
        {
            FilterDefinition<Document> docTBU = Builders<Document>.Filter.Eq(d => d._id, document._id);
            _mongoDBContext.document.ReplaceOne(docTBU, document);
        }
        #endregion

        #region // folder services
        // returns all sub-folders and documents in given folder
        public FolderDocument? GetAllFromPath(string folderPath, string requestorId)
        {
            // in case service is run by API and not user
            bool getAll = (requestorId == PermissionType.Admin.type);
            List<PermissionStore> permissions;
            // if folderPath is given find it by folder, if documentPath is given find it by document
            Folder? folder = _mongoDBContext.folder.Find(x => x.path_url == folderPath).SingleOrDefault();
            if (folder == null)
            {
                Document? document = _mongoDBContext.document.Find(x => x.path_url == folderPath).SingleOrDefault();
                if (document == null)
                    return null;

                // check permissions
                permissions = GetUserPermissionsByParentPathId(requestorId, document.folder_id);
                // get current folder (unless it is root), its subfolders and documents that exist in that folder
                return new FolderDocument(document,
                                          _mongoDBContext.folder.Find(x => x.parent_folder_id == document.folder_id).ToEnumerable(),
                                          _mongoDBContext.document.Find(x => x.folder_id == document.folder_id).ToEnumerable(),
                                          permissions,
                                          getAll);
            }

            // check permissions
            permissions = GetUserPermissionsByParentPathId(requestorId, folder._id);
            permissions.AddRange(GetUserPermissionsByParentPathId(requestorId, folder.parent_folder_id)); // add permissions from parent folder
            // get current folder (unless it is root), its subfolders and documents that exist in that folder
            return new FolderDocument(folder,
                                      _mongoDBContext.folder.Find(x => x.parent_folder_id == folder._id).ToEnumerable(),
                                      _mongoDBContext.document.Find(x => x.folder_id == folder._id).ToEnumerable(),
                                      permissions,
                                      getAll);
        }

        // returns hierarchy of given folder (shows from 'root' to given folder name)
        public List<HierarchyData>? GetHierarchy(string folderPath, string requestorId)
        {
            // from path get all folder names, thne start from root
            var pathArr = folderPath.Split("/").Where(elem => elem != "");
            var rootFolder = _mongoDBContext.folder.Find(x => x.parent_folder_id == Guid.Empty).Single();
            // get all root permissions
            var permissions = GetUserPermissionsByParentPathId(requestorId, rootFolder._id);

            return CreateHierarchy(rootFolder, pathArr.ToArray(), new List<HierarchyData>(), permissions);
        }

        // returns folder by name
        public Folder? GetFolderByName(string folderName) =>
            _mongoDBContext.folder.Find(x => x.folder_name == folderName).SingleOrDefault();

        // returns folder by path
        public Folder? GetFolderByPath(string folderPath) =>
            _mongoDBContext.folder.Find(x => x.path_url == folderPath).SingleOrDefault();

        // returns folder by id
        public Folder? GetFolderById(Guid folderId) =>
            _mongoDBContext.folder.Find(x => x._id == folderId).SingleOrDefault();

        public bool CheckFolderName(Folder folder, string name) =>
            _mongoDBContext.folder.Find(x => x.parent_folder_id == folder.parent_folder_id && x.folder_name == name).Any();

        // creates folder in database
        public Folder CreateFolder(Folder folder)
        {
            _mongoDBContext.folder.InsertOne(folder);
            return folder;
        }

        public Folder? RenameFolder(Folder folder, string name)
        {
            folder.folder_name = name;
            FilterDefinition<Folder> folderTBU = Builders<Folder>.Filter.Eq(d => d._id, folder._id);
            _mongoDBContext.folder.ReplaceOne(folderTBU, folder);
            return folder;
        }

        // rename subFolder and their documents paths
        public void RenamePath(Folder folder, string newFolderPath, string name)
        {
            // change folder path
            folder.path_url = $"{newFolderPath}/{name}";

            FilterDefinition<Folder> folderTBU = Builders<Folder>.Filter.Eq(d => d._id, folder._id);
            _mongoDBContext.folder.ReplaceOne(folderTBU, folder);

            // find all docuemnts and change their path_url
            IEnumerable<Document> documents = _mongoDBContext.document.Find(d => d.folder_id == folder._id).ToEnumerable();
            foreach (Document document in documents)
            {
                document.path_url = $"{folder.path_url}/{document.document_name}";
                FilterDefinition<Document> documentTBU = Builders<Document>.Filter.Eq(d => d._id, document._id);
                _mongoDBContext.document.ReplaceOne(documentTBU, document);
            }
            // find all subfolders and change their path_url
            IEnumerable<Folder> subFolders = _mongoDBContext.folder.Find(f => f.parent_folder_id == folder._id).ToEnumerable();
            foreach (Folder subFolder in subFolders)
                RenamePath(subFolder, folder.path_url, subFolder.folder_name);
        }

        // delete folder and all its subfolders (including all documents in folder and subfolders)
        public void DeleteFolder(Folder folder)
        {
            FilterDefinition<Folder> folderTBR = Builders<Folder>.Filter.Eq(d => d._id, folder._id);
            _mongoDBContext.folder.DeleteOne(folderTBR);
        }
        #endregion

        #region // search services
        // get selected size of paths that contain searchTerm
        public List<SearchPath> SearchByPath(int page, int pageSize, string searchTerm, string requestorId)
        {
            List<SearchPath>? folderPaths = _mongoDBContext.folder.Find(x => x.path_url.ToLower().Contains(searchTerm.ToLower())).ToList().
                                                               Select(x => new SearchPath { _id = x._id, name = x.folder_name, path_url = x.path_url, is_folder = true }).ToList();
            List<SearchPath>? documentPaths = _mongoDBContext.document.Find(x => x.path_url.Contains(searchTerm)).ToList().
                                                                   Select(x => new SearchPath { _id = x._id, name = x.document_name, path_url = x.path_url, is_folder = false }).ToList();
            // add together and return all paths
            folderPaths.AddRange(documentPaths);
            return folderPaths;
        }
        #endregion

        #region //permission services

        // check permissions
        public bool CheckRequestorPermissions(string requestorId, Guid? pathId, IEnumerable<string> permissions) =>
            permissions.Any(permission => _mongoDBContext.permission_store.Find(x => x.path_id == pathId &&
                                                                               (x.user_id == requestorId || x.user_id == AccessType.Public.type) && 
                                                                                x.permission == permission).Any());

        // return all permissions for given user
        public List<PermissionStore> GetUserPermissionsByParentPathId(string userId, Guid parentPathId) =>
            _mongoDBContext.permission_store.Find(x => x.parent_path_id == parentPathId && (x.user_id == userId || x.user_id == AccessType.Public.type)).ToList();

        // return permissions for given user
        public IEnumerable<PermissionStore> GetUserPermissionByPathId(string userId, Guid pathId) =>
            _mongoDBContext.permission_store.Find(x => x.path_id == pathId && (x.user_id == userId || x.user_id == AccessType.Public.type)).ToEnumerable();

        // return permissions for given path
        public IEnumerable<PermissionStore> GetPathPermission(Guid? pathId) =>
            _mongoDBContext.permission_store.Find(x => x.path_id == pathId).ToEnumerable();

        public void CreatePermission(List<PermissionStore> permissionStore) =>
            _mongoDBContext.permission_store.InsertMany(permissionStore);

        public void UpdatePermission(string userId, IEnumerable<string> permissions, Guid? pathId, string assignee)
        {
            // get all permissions connected to userId and pathId, then create list of only permissions from it
            IEnumerable<PermissionStore> permissionStore = _mongoDBContext.permission_store.Find(x => x.path_id == pathId && x.user_id == userId).ToEnumerable();
            IEnumerable<string> userPermissions = permissionStore.Select(x => x.permission);

            IEnumerable<string> permissionsTBD = userPermissions.Except(permissions); // permissions that will be removed
            foreach (string permission in permissionsTBD)
            {
                FilterDefinition<PermissionStore> permissionTBD = Builders<PermissionStore>.Filter.Eq(x => x._id, permissionStore.First(up => up.permission == permission)._id);
                _mongoDBContext.permission_store.DeleteOne(permissionTBD);
            }

            IEnumerable<string> permissionsTBU = permissions.Except(userPermissions); // permissions that will be added
            foreach (string permission in permissionsTBU)
            {
                PermissionStore newUserPermission = new()
                {
                    _id = Guid.NewGuid(),
                    user_id = userId,
                    path_id = pathId,
                    permission = permission,
                    assigned_by = assignee
                };
                _mongoDBContext.permission_store.InsertOne(newUserPermission);
            }
        }

        public void DeletePermission(string userId, Guid? pathId)
        {
            FilterDefinition<PermissionStore> permissionsTBD = Builders<PermissionStore>.Filter.Where(x => x.path_id == pathId && x.user_id == userId);
            _mongoDBContext.permission_store.DeleteMany(permissionsTBD);
        }

        #endregion

        #region // private functions
        // recursive function for finding child folders
        private List<HierarchyData> CreateHierarchy(Folder rootFolder, string[] pathArray, List<HierarchyData> hierarchyData, IEnumerable<PermissionStore> permissions)
        {
            var subFolders = _mongoDBContext.folder.Find(x => x.parent_folder_id == rootFolder._id).ToEnumerable();
            var documents = _mongoDBContext.document.Find(x => x.folder_id == rootFolder._id).ToEnumerable();

            foreach (var folder in subFolders)
            {
                // skip folder that has no permissions
                if (!permissions.Any(p => p.path_id == folder._id))
                    continue;
                // if there is still path (folder) in array and if folder's name is same as one from path then label folder as Expended
                if (pathArray.Any() && folder.folder_name == pathArray.First())
                {
                    hierarchyData.Add(new HierarchyData(folder, true));
                    continue;
                }
                hierarchyData.Add(new HierarchyData(folder, false));
            }
            foreach (var document in documents)
            {
                // skip document that has no permissions
                if (!permissions.Any(p => p.path_id == document._id))
                    continue;
                hierarchyData.Add(new HierarchyData(document));
            }

            // when all folders from folder path are checked, finish the recursion (base condition)
            if (!pathArray.Any())
                return hierarchyData;

            // root folder is becoming next Expended folder (folder in path)
            rootFolder = _mongoDBContext.folder.Find(x => x.parent_folder_id == rootFolder._id && x.folder_name == pathArray.First()).SingleOrDefault();
            if (rootFolder == null)
                return hierarchyData;
            // get permission for next root folder
            permissions = GetUserPermissionsByParentPathId(permissions.First().user_id, rootFolder._id);
            // remove checked folder from path
            pathArray = pathArray.Where(w => w != pathArray[0]).ToArray();

            // recursion..
            hierarchyData.First(x => x._id == rootFolder._id).childFolder = CreateHierarchy(rootFolder, pathArray, new List<HierarchyData>(), permissions);
            return hierarchyData;
        }
        #endregion
    }
}
