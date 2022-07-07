using DocumentService.PolyglotPersistence.DBContext;
using DocumentService.PolyglotPersistence.IRepositories;
using DocumentService.PolyglotPersistence.IServices;
using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.PermissionModels;
using DocumentService.PolyglotPersistence.Models.ResponseModels;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DocumentService.PolyglotPersistence.Repositories
{
    public class SqlRepository : IRepository
    {
        private readonly SqlContext _sqlContext;
        private readonly IAzureBlobServices _azureBlobService;
        private readonly IConfiguration _configuration;
        private readonly CloudBlobContainer _documentContentContainer;
        private readonly CloudBlobContainer _historyContentContainer;

        private static string Database => new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("DatabaseType").Value;
        private string AzureBlobState => _configuration.GetSection("AzureBlobStorage").GetSection("Actived").Value;
        private string AzureDocumentContainerName => _configuration.GetSection("AzureBlobStorage").GetSection("DocumentContainer").Value;
        private string AzureHistoryContainerName => _configuration.GetSection("AzureBlobStorage").GetSection("HistoryContainer").Value;

        public SqlRepository(SqlContext sqlContext, IAzureBlobServices azureBlobService, IConfiguration configuration)
        {
            _sqlContext = sqlContext;
            _configuration = configuration;

            _azureBlobService = azureBlobService;
            _documentContentContainer = _azureBlobService.ConnectToAzureBlobStorage(AzureDocumentContainerName);
            _historyContentContainer = _azureBlobService.ConnectToAzureBlobStorage(AzureHistoryContainerName);
        }

        #region //documents services
        // returns document by Id 
        public Document? GetDocument(Guid documentId) =>
            _sqlContext.document.SingleOrDefault(x => x._id == documentId);

        // return document by path
        public bool CheckDocumentName(Guid parentFolderId, string name) =>
            _sqlContext.document.Any(x => x.folder_id == parentFolderId && x.document_name == name);

        // creates document
        public void CreateDocument(Document document)
        {
            _sqlContext.document.Add(document);
            _sqlContext.SaveChanges();
        }

        // updates document
        public void CreateDocumentVersion(DocumentHistory documentVersion)
        {
            _sqlContext.document_history.Add(documentVersion);
            // check if document exists in document content or AzureBlob
            if (_sqlContext.document_content.Any(x => x._id == documentVersion.document_id))
            {
                // create initial document content in history
                DocumentHistoryContent documentHistoryContent = new()
                {
                    _id = documentVersion._id,
                    document_id = documentVersion.document_id
                };
                _sqlContext.document_history_content.Add(documentHistoryContent);
                _sqlContext.SaveChanges();

                // move existing content from Content Table to HistoryContent Table
                if (Database == "PostgreSql")
                    CreateVersionInPostgreSql(documentVersion.document_id, _sqlContext.DocumentHistoryContentTableName, _sqlContext.DocumentContentTableName);
                else if (Database == "MySql")
                    CreateVersionInMySql(documentVersion.document_id, _sqlContext.DocumentHistoryContentTableName, _sqlContext.DocumentContentTableName);
                else
                    CreateVersionInMsSql(documentVersion.document_id, _sqlContext.DocumentHistoryContentTableName, _sqlContext.DocumentContentTableName);
            }
            else
                _azureBlobService.CopyBlobContent(_documentContentContainer, _historyContentContainer, documentVersion.document_id.ToString());
        }

        // rename document
        public void RenameDocument(Document document, string name)
        {
            _sqlContext.document.Update(document);
            _sqlContext.SaveChanges();
        }

        // delete document
        public void DeleteDocument(Document document, bool deleteHistory)
        {
            Document documentTBD = new() { _id = document._id };
            _sqlContext.document.Attach(documentTBD);
            _sqlContext.document.Remove(documentTBD);
            DocumentContent documentContentTBD = new() { _id = document._id };
            _sqlContext.document_content.Attach(documentContentTBD);
            _sqlContext.document_content.Remove(documentContentTBD);

            // azure document blob container
            _azureBlobService.DeleteBlob(_documentContentContainer, document._id.ToString());

            if (deleteHistory)
            {
                DocumentHistory documentHistoryTBD = new() { document_id = document._id };
                _sqlContext.document_history.Attach(documentHistoryTBD);
                _sqlContext.document_history.RemoveRange(documentHistoryTBD);
                DocumentHistoryContent documentHistoryContentTBD = new() { document_id = document._id };
                _sqlContext.document_history_content.Attach(documentHistoryContentTBD);
                _sqlContext.document_history_content.RemoveRange(documentHistoryContentTBD);

                // azure history blob container
                _azureBlobService.DeleteBlob(_historyContentContainer, document._id.ToString());
            }
        }

        public void RestoreDocument(Document document)
        {
            // find latest version in history and add it to document table
            DocumentHistory documentVersion = _sqlContext.document_history.Single(dh => dh.document_id == document._id && dh.version == document.version);
            _sqlContext.document.Add(document);

            // check if document exists in document content or AzureBlob
            if (_sqlContext.document_history_content.Any(x => x._id == documentVersion._id))
            {
                // create initial document content in history
                DocumentContent documentContent = new()
                {
                    _id = documentVersion._id
                };
                _sqlContext.document_content.Add(documentContent);
                _sqlContext.SaveChanges();

                // move existing content from Content Table to HistoryContent Table
                if (Database == "PostgreSql")
                    CreateVersionInPostgreSql(documentVersion.document_id, _sqlContext.DocumentContentTableName, _sqlContext.DocumentHistoryContentTableName);
                else if (Database == "MySql")
                    CreateVersionInMySql(documentVersion.document_id, _sqlContext.DocumentContentTableName, _sqlContext.DocumentHistoryContentTableName);
                else
                    CreateVersionInMsSql(documentVersion.document_id, _sqlContext.DocumentContentTableName, _sqlContext.DocumentHistoryContentTableName);

                // remove latest version from document_history_content
                DocumentHistoryContent documentHistoryContentTBD = new() { _id = documentVersion._id };
                _sqlContext.document_history_content.Attach(documentHistoryContentTBD);
                _sqlContext.document_history_content.Remove(documentHistoryContentTBD);
            }
            else
            {
                _azureBlobService.CopyBlobContent(_historyContentContainer, _documentContentContainer, documentVersion._id.ToString());
                _azureBlobService.DeleteBlob(_historyContentContainer, documentVersion._id.ToString());
            }

            // remove latest version from document_history
            DocumentHistory documentHistoryTBD = new() { _id = documentVersion._id };
            _sqlContext.document_history.Attach(documentHistoryTBD);
            _sqlContext.document_history.Remove(documentHistoryTBD);
            _sqlContext.SaveChanges();
        }
        #endregion

        #region //document content services
        // get document content in chunks
        public byte[] GetDocumentContentInChunks(Guid? documentId, int offset, int chunk_size)
        {
            // if document is not located in document_content, get it from AzureBlob storage
            if (!_sqlContext.document_content.Any(x => x._id == documentId))
            {
                byte[] buffer = new byte[chunk_size];
                var cloudClient = _documentContentContainer.GetBlobReference(documentId.ToString());
                cloudClient.DownloadRangeToByteArray(buffer, 0, offset, chunk_size);

                return buffer;
            }
            // since index starts at 1 in sql
            offset += 1;

            IQueryable<DbBuffer> content;
            if (Database == "PostgreSql")
                content = GetContentFromPostgreSql(documentId, offset, chunk_size, _sqlContext.DocumentContentTableName);
            else if (Database == "MySql")
                content = GetContentFromMySql(documentId, offset, chunk_size, _sqlContext.DocumentContentTableName);
            else
                content = GetContentFromMsSql(documentId, offset, chunk_size, _sqlContext.DocumentContentTableName);

            return content.First().substring;
        }

        // create document content
        public void CreateDocumentContent(DocumentContent documentContent)
        {
            _sqlContext.document_content.Add(documentContent);
            _sqlContext.SaveChanges();
        }

        // create document content in chunks (SQL or Azure) -- can be hybrid
        public int CreateDocumentContentInChunks(Guid documentId, byte[] buffer, ref HashSet<string> blocklist)
        {
            // if stream is empty, return 0 as sign to stop uploading/updating
            if (buffer.Length == 0)
                return 0;

            if (AzureBlobState == "N")
            {
                if (Database == "PostgreSql")
                    CreateContentInPostgreSql(documentId, buffer);
                else if (Database == "MySql")
                    CreateContentInMySql(documentId, buffer);
                else
                    CreateContentInMsSql(documentId, buffer);
            }
            // azure blob storage
            else
                blocklist = _azureBlobService.UploadToAzureBlobStorage(_documentContentContainer, documentId.ToString(), buffer, blocklist);

            // return buffer size
            return buffer.Length;
        }
        #endregion

        #region //history services
        // returns all document versions of given id 
        public IEnumerable<DocumentHistory> GetDocumentHistory(Guid? documentId) =>
            _sqlContext.document_history.Where(x => x.document_id == documentId);

        // returns content of requested document version
        public DocumentHistory? GetDocumentByVersionHistory(Guid? documentId, int version) =>
            _sqlContext.document_history.SingleOrDefault(x => x.document_id == documentId && x.version == version);

        // get history version buffer
        public byte[] GetDocumentContentVersionInChunks(Guid? documentId, int offset, int chunk_size)
        {
            // if document is not located in document_content, get it from AzureBlob storage
            if (!_sqlContext.document_history_content.Any(x => x._id == documentId))
            {
                byte[] buffer = new byte[chunk_size];
                var cloudClient = _historyContentContainer.GetBlobReference(documentId.ToString());
                cloudClient.DownloadRangeToByteArray(buffer, 0, offset, chunk_size);

                return buffer;
            }
            // since index starts at 1 in sql
            offset += 1;
            
            IQueryable<DbBuffer> content;
            if (Database == "PostgreSql")
                content = GetContentFromPostgreSql(documentId, offset, chunk_size, _sqlContext.DocumentHistoryContentTableName);
            else if (Database == "MySql")
                content = GetContentFromMySql(documentId, offset, chunk_size, _sqlContext.DocumentHistoryContentTableName);
            else
                content = GetContentFromMsSql(documentId, offset, chunk_size, _sqlContext.DocumentHistoryContentTableName);

            return content.First().substring;
        }
        #endregion

        #region //check in/out services
        // does checkin and returns updated document if successful
        public void CheckInDocument(Document document)
        {
            _sqlContext.document.Update(document);
            _sqlContext.SaveChanges();
        }

        // does checkout and returns updated document if successful
        public void CheckOutDocument(Document document, string requestorId)
        {
            _sqlContext.document.Update(document);
            _sqlContext.SaveChanges();
        }
        #endregion

        #region //folder services
        // returns all sub-folders and documents in given folder
        public FolderDocument? GetAllFromPath(string folderPath, string requestorId)
        {
            // in case service is run by API and not user
            bool getAll = (requestorId == PermissionType.Admin.type);
            List<PermissionStore> permissions;

            // if folderPath is given find it by folder, if documentPath is given find it by document
            var folder = _sqlContext.folder.SingleOrDefault(x => x.path_url == folderPath);
            if (folder == null)
            {
                var document = _sqlContext.document.SingleOrDefault(x => x.path_url == folderPath);
                if (document == null)
                    return null;

                // take all permission from current requestor
                permissions = GetUserPermissionsByParentPathId(requestorId, document.folder_id);
                // get current folder (unless it is root), its subfolders and documents that exist in that folder
                return new FolderDocument(document,
                                          _sqlContext.folder.Where(x => x.parent_folder_id == document.folder_id).AsEnumerable(),
                                          _sqlContext.document.Where(x => x.folder_id == document.folder_id).AsEnumerable(),
                                          permissions,
                                          getAll);
            }

            // check permissions
            permissions = GetUserPermissionsByParentPathId(requestorId, folder._id);
            permissions.AddRange(GetUserPermissionByPathId(requestorId, folder.parent_folder_id));
            // get current folder (unless it is root), its subfolders and documents that exist in that folder
            return new FolderDocument(folder,
                                      _sqlContext.folder.Where(x => x.parent_folder_id == folder._id).AsEnumerable(),
                                      _sqlContext.document.Where(x => x.folder_id == folder._id).AsEnumerable(),
                                      permissions,
                                      getAll);
        }

        // returns hierarchy of given folder (shows from 'root' to given folder name)
        public List<HierarchyData>? GetHierarchy(string folderPath, string requestorId)
        {
            // from path get all folder names, thne start from root
            var pathArr = folderPath.Split("/").Where(elem => elem != "");
            var rootFolder = _sqlContext.folder.First(x => x.parent_folder_id == Guid.Empty);
            // get all root permissions
            var permissions = GetUserPermissionsByParentPathId(requestorId, rootFolder._id);

            return CreateHierarchy(rootFolder, pathArr.ToArray(), new List<HierarchyData>(), permissions);
        }

        // returns folder by name
        public Folder? GetFolderByName(string folderName) =>
            _sqlContext.folder.SingleOrDefault(x => x.folder_name == folderName);

        // returns folder path
        public Folder? GetFolderByPath(string folderPath) =>
            _sqlContext.folder.SingleOrDefault(x => x.path_url == folderPath);

        // returns folder by id
        public Folder? GetFolderById(Guid folderId) =>
            _sqlContext.folder.SingleOrDefault(x => x._id == folderId);

        public bool CheckFolderName(Folder folder, string name) =>
            _sqlContext.folder.Any(x => x.folder_name == name && x.parent_folder_id == folder.parent_folder_id);

        // creates folder in database
        public Folder CreateFolder(Folder folder)
        {
            _sqlContext.folder.Add(folder);
            _sqlContext.SaveChanges();
            return folder;
        }

        public Folder? RenameFolder(Folder folder, string name)
        {
            folder.folder_name = name;
            _sqlContext.folder.Update(folder);
            _sqlContext.SaveChanges();
            return folder;
        }

        // rename subFolders and their documents path
        public void RenamePath(Folder folder, string newFolderPath, string name)
        {
            // change folder path
            folder.path_url = $"{newFolderPath}/{name}";
            _sqlContext.folder.Update(folder);

            // find all docuemnts and change their path_url
            IEnumerable<Document> documents = _sqlContext.document.Where(d => d.folder_id == folder._id);
            foreach (var document in documents)
            {
                document.path_url = $"{folder.path_url}/{document.document_name}";
                _sqlContext.document.Update(document);
            }
            // find all subfolders and change their path_url
            IEnumerable<Folder> subFolders = _sqlContext.folder.Where(f => f.parent_folder_id == folder._id);
            foreach (var subFolder in subFolders)
                RenamePath(subFolder, folder.path_url, subFolder.folder_name);
        }

        // delete folder and all its subfolders (including all documents in folder and subfolders)
        public void DeleteFolder(Folder folder)
        {
            _sqlContext.folder.Remove(folder);
            _sqlContext.SaveChanges();
        }
        #endregion

        #region //search services
        // get selected size of paths that contain searchTerm
        public List<SearchPath> SearchByPath(int page, int pageSize, string searchTerm, string requestorId)
        {
            List<SearchPath>? folderPaths = _sqlContext.folder.Where(x => x.path_url.Contains(searchTerm)).
                                                    Select(x => new SearchPath { _id = x._id, name = x.folder_name, path_url = x.path_url, is_folder = true }).ToList();
            List<SearchPath>? documentPaths = _sqlContext.document.Where(x => x.path_url.Contains(searchTerm)).
                                                        Select(x => new SearchPath { _id = x._id, name = x.document_name, path_url = x.path_url, is_folder = false }).ToList();
            // add together and return all paths
            folderPaths.AddRange(documentPaths);
            return folderPaths;
        }
        #endregion

        #region //permission services
        // check requestor permissions
        public bool CheckRequestorPermissions(string requestorId, Guid? pathId, IEnumerable<string> permissions) =>
            permissions.Any(permission => _sqlContext.permission_store.Any(x => x.path_id == pathId &&
                                                                                 (x.user_id == requestorId || x.user_id == AccessType.Public.type) &&
                                                                                  x.permission == permission));

        // return all permissions for given user
        public List<PermissionStore> GetUserPermissionsByParentPathId(string userId, Guid parentPathId) =>
            _sqlContext.permission_store.Where(x => x.parent_path_id == parentPathId && (x.user_id == userId || x.user_id == AccessType.Public.type)).ToList();

        // return permissions for given user on given path
        public IEnumerable<PermissionStore> GetUserPermissionByPathId(string userId, Guid pathId) =>
            _sqlContext.permission_store.Where(x => x.path_id == pathId && (x.user_id == userId || x.user_id == AccessType.Public.type));

        // return permissions for given path
        public IEnumerable<PermissionStore> GetPathPermission(Guid? pathId) =>
            _sqlContext.permission_store.Where(x => x.path_id == pathId);

        // create permissions
        public void CreatePermission(List<PermissionStore> permissionStore)
        {
            _sqlContext.permission_store.AddRange(permissionStore);
            _sqlContext.SaveChanges();
        }

        // update permissions (delete missing ones, add new ones)
        public void UpdatePermission(string userId, IEnumerable<string> permissions, Guid? pathId, string assignee)
        {
            // get all permissions connected to userId and pathId, then create list of only permissions from it
            IEnumerable<PermissionStore> permissionStore = _sqlContext.permission_store.Where(x => x.path_id == pathId && x.user_id == userId);
            IEnumerable<string> userPermissions = permissionStore.Select(x => x.permission);

            IEnumerable<string> permissionsTBD = userPermissions.Except(permissions); // permissions that will be removed
            foreach (string permission in permissionsTBD)
            {
                _sqlContext.permission_store.Remove(permissionStore.First(ps => ps.permission == permission));
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
                _sqlContext.permission_store.Add(newUserPermission);
            }
            _sqlContext.SaveChanges();
        }

        // delete permissions
        public void DeletePermission(string userId, Guid? pathId)
        {
            IEnumerable<PermissionStore> userPermissions = _sqlContext.permission_store.Where(x => x.path_id == pathId && x.user_id == userId).AsEnumerable();
            _sqlContext.permission_store.RemoveRange(userPermissions);
            _sqlContext.SaveChanges();
        }
        #endregion

        #region // private functions

        // Create content in document history
        private void CreateVersionInPostgreSql(Guid documentId, string update_table, string from_table)
        {
            NpgsqlParameter document_id = new("@document_id", NpgsqlDbType.Uuid);
            document_id.Value = documentId;
            _sqlContext.Database.ExecuteSqlRaw($@"UPDATE {update_table}
                                                SET content={from_table}.content, content_type={from_table}.content_type
                                                FROM {from_table}
                                                WHERE {from_table}._id={{0}}", document_id); // WHERE document_content._id = document_id
        }
        private void CreateVersionInMySql(Guid documentId, string update_table, string from_table)
        {
            MySqlParameter document_id = new("@document_id", MySqlDbType.VarChar);
            document_id.Value = documentId;
            _sqlContext.Database.ExecuteSqlRaw($@"UPDATE {update_table}
                                                SET content={from_table}.content, content_type={from_table}.content_type
                                                FROM {from_table}
                                                WHERE {from_table}._id={{0}}", document_id);
        }
        private void CreateVersionInMsSql(Guid documentId, string update_table, string from_table)
        {
            SqlParameter document_id = new("@document_id", SqlDbType.UniqueIdentifier);
            document_id.Value = documentId;
            _sqlContext.Database.ExecuteSqlRaw($@"UPDATE {update_table}
                                                SET content={from_table}.content, content_type={from_table}.content_type
                                                FROM {from_table}
                                                WHERE {from_table}._id={{0}}", document_id);
        }

        // Get content from different SQL databases
        private IQueryable<DbBuffer> GetContentFromPostgreSql(Guid? documentId, int offset, int chunk_size, string from_table)
        {
            NpgsqlParameter document_id = new("@document_id", NpgsqlDbType.Uuid);
            document_id.Value = documentId;
            NpgsqlParameter content_offset = new("@content_offset", NpgsqlDbType.Integer);
            content_offset.Value = offset;
            NpgsqlParameter pg_chunk_size = new("@chunk_size", NpgsqlDbType.Integer);
            pg_chunk_size.Value = chunk_size;
            // Get document content substring which will be streamed to a file
            return _sqlContext.dbBuffer.FromSqlRaw($"SELECT substring(content::bytea from {{0}} for {{1}}) FROM {from_table} WHERE _id={{2}}", content_offset, pg_chunk_size, document_id).AsNoTracking();
        }
        private IQueryable<DbBuffer> GetContentFromMySql(Guid? documentId, int offset, int chunk_size, string from_table)
        {
            MySqlParameter document_id = new("@document_id", MySqlDbType.VarChar);
            document_id.Value = documentId;
            MySqlParameter content_offset = new("@content_offset", MySqlDbType.Int64);
            content_offset.Value = offset;
            MySqlParameter pg_chunk_size = new("@chunk_size", MySqlDbType.Int64);
            pg_chunk_size.Value = chunk_size;
            // Get document content substring which will be streamed to a file
            return _sqlContext.dbBuffer.FromSqlRaw($"SELECT substring(content from {{0}} for {{1}}) AS substring FROM {from_table} WHERE _id={{2}}", content_offset, pg_chunk_size, document_id).AsNoTracking();
        }
        private IQueryable<DbBuffer> GetContentFromMsSql(Guid? documentId, int offset, int chunk_size, string from_table)
        {
            SqlParameter document_id = new("@document_id", SqlDbType.UniqueIdentifier);
            document_id.Value = documentId;
            SqlParameter content_offset = new("@content_offset", SqlDbType.Int);
            content_offset.Value = offset;
            SqlParameter pg_chunk_size = new("@chunk_size", SqlDbType.Int);
            pg_chunk_size.Value = chunk_size;
            // Get document content substring which will bestreamed to a file
            return _sqlContext.dbBuffer.FromSqlRaw($"SELECT substring(content,{{0}},{{1}}) AS substring FROM {from_table} WHERE _id={{2}}", content_offset, pg_chunk_size, document_id).AsNoTracking();
        }

        // Create content for different sql dbs
        private void CreateContentInPostgreSql(Guid documentId, byte[] buffer)
        {
            // assigne database values/types
            NpgsqlParameter document_id = new("@document_id", NpgsqlDbType.Uuid);
            document_id.Value = documentId;
            NpgsqlParameter content_arr = new("@content_arr", NpgsqlDbType.Bytea);
            content_arr.Value = buffer;
            // Update document content with new buffer. ' || ' sign is addition operator
            _sqlContext.Database.ExecuteSqlRaw("UPDATE document_content SET content = content || {0} WHERE _id = {1};", content_arr, document_id);
        }
        private void CreateContentInMySql(Guid documentId, byte[] buffer)
        {
            // assigne database values/types
            MySqlParameter document_id = new("@document_id", MySqlDbType.VarChar);
            document_id.Value = documentId;
            MySqlParameter content_arr = new("@content_arr", MySqlDbType.LongBlob);
            content_arr.Value = buffer;
            // Update document content with new buffer. ' || ' sign is addition operator
            _sqlContext.Database.ExecuteSqlRaw("UPDATE document_content SET content = CONCAT(content, {0}) WHERE _id = {1};", content_arr, document_id);
        }
        private void CreateContentInMsSql(Guid documentId, byte[] buffer)
        {
            // assigne database values/types
            SqlParameter document_id = new("@document_id", SqlDbType.UniqueIdentifier);
            document_id.Value = documentId;
            SqlParameter content_arr = new("@content_arr", SqlDbType.VarBinary);
            content_arr.Value = buffer;
            // Update document content with new buffer. ' || ' sign is addition operator
            _sqlContext.Database.ExecuteSqlRaw("UPDATE document_content SET content = content + {0} WHERE _id = {1};", content_arr, document_id);
        }

        // recursive function for finding child folders
        private List<HierarchyData> CreateHierarchy(Folder rootFolder, string[] pathArray, List<HierarchyData> hierarchyData, IEnumerable<PermissionStore> permissions)
        {
            var subFolders = _sqlContext.folder.Where(x => x.parent_folder_id == rootFolder._id).AsEnumerable();
            var documents = _sqlContext.document.Where(x => x.folder_id == rootFolder._id).AsEnumerable();

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
            rootFolder = _sqlContext.folder.Single(x => x.parent_folder_id == rootFolder._id && x.folder_name == pathArray.SingleOrDefault());
            if (rootFolder == null)
                return hierarchyData;
            // get permission for next root folder
            permissions = GetUserPermissionsByParentPathId(permissions.First().user_id, rootFolder._id);
            // remove checked folder from path
            pathArray = pathArray.Where(w => w != pathArray[0]).ToArray();

            // recursion..
            hierarchyData.Single(x => x._id == rootFolder._id).childFolder = CreateHierarchy(rootFolder, pathArray, new List<HierarchyData>(), permissions);
            return hierarchyData;
        }
        #endregion
    }
}
