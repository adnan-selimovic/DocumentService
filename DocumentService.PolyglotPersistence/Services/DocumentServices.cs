using DocumentService.PolyglotPersistence.IRepositories;
using DocumentService.PolyglotPersistence.IServices;
using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.Elasticsearch;
using DocumentService.PolyglotPersistence.Models.ErrorModels;
using DocumentService.PolyglotPersistence.Models.PermissionModels;
using DocumentService.PolyglotPersistence.Models.RequestModel;
using DocumentService.PolyglotPersistence.Models.ResponseModels;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Nest;

namespace DocumentService.PolyglotPersistence.Services
{
    public class DocumentServices : IDocumentServices
    {
        private readonly IRepository _repository;
        private readonly IElasticSearchServices _elasticSearchService;

        public DocumentServices(IRepository repository, IElasticSearchServices elasticSearchService)
        {
            _repository = repository;
            _elasticSearchService = elasticSearchService;
        }

        #region // DocumentController services

        public void CheckInDocument(ref Document document)
        {
            document.checked_out_by = null;
            document.checked_out_date = null;
            _repository.CheckInDocument(document);
        }

        public void CheckOutDocument(ref Document document, string requestorId)
        {
            document.checked_out_by = requestorId;
            document.checked_out_date = DateTime.UtcNow;
            _repository.CheckOutDocument(document, requestorId);
        }

        // returns path (as string) to location of streamed content
        public string? GetPathOfStreamedDocument(string documentName, int documentSize, Guid documentId, int? version)
        {
            // find current path and create files directory if it does not exist
            string currentPath = Directory.GetCurrentDirectory();
            string folderPath = currentPath + "\\files";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // create new file where byte chunks will be written to
            string filePath = $"{folderPath}\\{documentName}";

            // check for files in directory and if they are older then ** remove them
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            foreach (FileInfo file in directoryInfo.GetFiles())
                if (file.Name + file.Extension.ToLower() == documentName)
                    return filePath;
                else if ((DateTime.Now - file.CreationTime).TotalMinutes > 10)
                    file.Delete();

            // create point to function
            Func<Guid?, int, int, byte[]>? GetDocumentContentFunction = null;
            // if version is not declared, get original document chunks, otherwise given document version
            GetDocumentContentFunction = (version == null) ? _repository.GetDocumentContentInChunks : _repository.GetDocumentContentVersionInChunks;

            StreamDocumentContent(filePath, documentSize, documentId, GetDocumentContentFunction);
            long length = new System.IO.FileInfo(filePath).Length;

            // check if upload is completed and return path
            return (length == documentSize) ? filePath : null;
        }

        public IEnumerable<DocumentHistory> GetDocumentHistory(Guid documentId) =>
            _repository.GetDocumentHistory(documentId);

        public void RenameDocument(ref Document document, string name)
        {
            string newName = $"{name}." + document.document_name.Split('.').Last();
            document.path_url = document.path_url.Replace(document.document_name, newName);
            document.document_name = newName;

            _repository.RenameDocument(document, name);
        }

        public Document? UpdateDocument(MultipartSection section, Document document, ContentDispositionHeaderValue contentDisposition, string requestorId)
        {
            DocumentHistory documentVersion = new()
            {
                _id = Guid.NewGuid(),
                document_id = document._id,
                document_name = document.document_name,
                content_type = document.content_type,
                document_size = document.document_size,
                path_url = document.path_url,
                created_by = document.created_by,
                period_from = document.created_date,
                period_to = DateTime.UtcNow,
                version = document.version
            };

            // save current version in history, then remove current content
            _repository.CreateDocumentVersion(documentVersion);
            _repository.DeleteDocument(document, false);

            document.path_url = document.path_url.Replace(document.document_name, contentDisposition.FileName.Value);
            document.document_name = contentDisposition.FileName.Value;
            document.created_by = requestorId;
            document.created_date = DateTime.UtcNow;
            document.version++;

            // upload new document
            return UploadDocumentContent(section, ref document) ? document : null;
        }

        public async Task<UploadResponse> UploadDocument(MultipartReader multipartReader, Folder folder, IEnumerable<Document> existingDocuments, string requestorId)
        {
            // create new response
            UploadResponse response = new();
            MultipartSection? section = await multipartReader.ReadNextSectionAsync();

            // while there is form-data in request go through upload/update process
            while (section != null)
            {
                // get header information
                _ = ContentDispositionHeaderValue.TryParse(
                       section.ContentDisposition, out var contentDisposition);

                if (contentDisposition == null)
                {
                    response.numberOfFailed++;
                    response.failedDocuments.Add(new ErrorResponse("", ErrorType.FailedToUpdateDocument));
                    section = await multipartReader.ReadNextSectionAsync();
                    continue;
                }

                // define data from header 
                Guid id = Guid.NewGuid();
                string file_name = contentDisposition.FileName.Value;
                string extension = Path.GetExtension(file_name).ToLowerInvariant();
                string content_type = MimeMapping.MimeUtility.GetMimeMapping(extension);
                int version = 1;
                string? checked_out_by = null;
                DateTime? checked_out_date = null;

                // check if document already exists in given path, if does then update it
                Document? existingDocument = existingDocuments.SingleOrDefault(x => x.document_name == file_name);
                if (existingDocument != null)
                {
                    // if document is not checked out, return error
                    if (existingDocument.checked_out_by != requestorId)
                    {
                        response.numberOfFailed++;
                        if (existingDocument.checked_out_by == null)
                            response.failedDocuments.Add(new ErrorResponse(existingDocument.document_name, (existingDocument.checked_out_by == null) ? ErrorType.NotCheckedOut : ErrorType.AlreadyCheckedOut));
                        section = await multipartReader.ReadNextSectionAsync();
                        continue;
                    }

                    DocumentHistory documentVersion = new()
                    {
                        _id = Guid.NewGuid(),
                        document_id = existingDocument._id,
                        document_name = existingDocument.document_name,
                        content_type = existingDocument.content_type,
                        document_size = existingDocument.document_size,
                        path_url = existingDocument.path_url,
                        created_by = existingDocument.created_by,
                        period_from = existingDocument.created_date,
                        period_to = DateTime.UtcNow,
                        version = existingDocument.version
                    };

                    _repository.CreateDocumentVersion(documentVersion);
                    _repository.DeleteDocument(existingDocument, false);
                    id = existingDocument._id;
                    version = existingDocument.version + 1;
                    checked_out_by = existingDocument.checked_out_by;
                    checked_out_date = existingDocument.checked_out_date;
                }

                // create initial version of document
                Document document = new()
                {
                    _id = id,
                    document_name = file_name,
                    content_type = content_type,
                    document_size = 0, // this will be calculated later
                    path_url = (folder.parent_folder_id == Guid.Empty) ? folder.path_url + file_name : $"{folder.path_url}/{file_name}", // performs check if document is created in root
                    created_by = requestorId,
                    folder_id = folder._id,
                    version = version,
                    checked_out_by = checked_out_by,
                    checked_out_date = checked_out_date
                };

                if (UploadDocumentContent(section, ref document))
                {
                    // if update is happening
                    if (existingDocument != null)
                    {
                        response.numberOfUpdates++;
                        response.updatedDocuments.Add(document);

                        // move to next section and continue
                        section = await multipartReader.ReadNextSectionAsync();
                        continue;
                    }
                    // if document is being uploaded
                    List<PermissionStore> permission = new()
                        {
                            new PermissionStore()
                            {
                                _id = Guid.NewGuid(),
                                user_id = requestorId,
                                permission = PermissionType.Owner.type,
                                path_id = document._id,
                                parent_path_id = document.folder_id,
                                assigned_by = requestorId
                            }
                        };
                    _repository.CreatePermission(permission);

                    response.numberOfUploads++;
                    response.uploadedDocuments.Add(document);
                }
                // if upload failes, report with error type and document that failed
                else
                {
                    // Restore document latest version in case update failes, or in case of upload, remove all unfinished entities
                    if (existingDocument != null)
                        _repository.RestoreDocument(existingDocument);
                    else
                        _repository.DeleteDocument(document, false);

                    response.numberOfFailed++;
                    response.failedDocuments.Add(new ErrorResponse(document.document_name, (existingDocument != null) ? ErrorType.FailedToUpdateDocument : ErrorType.FailedToUploadDocument));
                }

                // read next form data
                section = await multipartReader.ReadNextSectionAsync();
            }

            return response;
        }

        public void DeleteDocument(Document document)
        {
            ElasticClient client = _elasticSearchService.SetupElasticSearchClient();
            _repository.DeleteDocument(document, true);
            // if document type is text (i.e. stored in elastic search server, remove it)
            if (client.DocumentExists<ElasticDocument>(document._id).Exists)
                client.Delete<ElasticDocument>(document._id);
        }

        public PageResponse<ElasticDocument> SearchDocument(SearchQuery query, string requestorId)
        {
            // create elastic client and search for through document contents for given searchTerm (Word, text...)
            ElasticClient client = _elasticSearchService.SetupElasticSearchClient();

            // finds all matches and gives total count
            CountResponse totalCount = client.Count<ElasticDocument>(c => c.Query(q => q.Match(m => m.Field(f => f.content).Query(query.searchTerm))));

            // select fields (object attributes) that need to be returned and find document that match given searchTerm (return just given pageSize of documents)
            ISearchResponse<ElasticDocument> searchResponse = client.Search<ElasticDocument>(s => s
                .Source(sf => sf.Includes(i => i
                                .Fields(f => f.id, f => f.content, f => f.name, f => f.path_url)))
                .Query(q => q.Match(m => m.
                              Field(f => f.content).Query(query.searchTerm))).Size(query.pageSize).Skip((query.page - 1) * query.pageSize)
            );

            return new PageResponse<ElasticDocument>()
            {
                totalSize = totalCount.Count,
                items = searchResponse.Documents.Where(d => _repository.CheckRequestorPermissions(requestorId, d.id, PermissionType.Read.group)) // return only those on which user has read permission
            };
        }

        public List<HierarchyData>? GetPathHierarchy(string folderPath, string requestorId) =>
            _repository.GetHierarchy(folderPath, requestorId);

        #endregion

        #region // Helper services
        public Document? GetDocument(Guid documentId) =>
            _repository.GetDocument(documentId);

        public bool CheckDocumentName(Guid parentFolderId, string name) =>
            _repository.CheckDocumentName(parentFolderId, name);

        public DocumentHistory? GetDocumentVersion(Guid documentId, int version) =>
            _repository.GetDocumentByVersionHistory(documentId, version);
        #endregion

        #region // Private functions
        private bool UploadDocumentContent(MultipartSection section, ref Document document)
        {
            try
            {
                // save content into database
                int chunk_size = 4 * 1024 * 1024; // chunk size -- required to determine when is end of stream
                int total_size = 0; // total size of stream
                HashSet<string> blocklist = new(); // for AzureBlob storage blockIds
                using (BinaryReader reader = new(section.Body))
                {
                    byte[] buffer = reader.ReadBytes(chunk_size);
                    // create initial document_content version
                    DocumentContent documentContent = new()
                    {
                        _id = document._id,
                        content = new byte[0],
                        content_type = document.content_type
                    };

                    // if total size is same as upload(chunk) size then continue with upload (even if it is same, i.e. there is no more stream, this will continue and append 0 bytes at the end) 
                    if (buffer.Length == chunk_size)
                    {
                        _repository.CreateDocumentContent(documentContent); // initialy create document_content in database, so stream chunks can be added to it
                        while (chunk_size != 0)
                        {
                            chunk_size = _repository.CreateDocumentContentInChunks(document._id, buffer, ref blocklist); // add chunk of stream (buffer) in database to existing document
                            total_size += buffer.Length; // add to total document size
                            buffer = reader.ReadBytes(chunk_size); // read new chunk of stream
                        }
                        reader.Dispose();
                    }
                    // if total size is less then chunk_size then upload document normal way (i.e just once) -- buffer.length cant go over chunk_size since it is limited by it
                    else
                    {
                        // assign buffer content to document content
                        documentContent.content = buffer;
                        total_size = buffer.Length;
                        _repository.CreateDocumentContent(documentContent);
                    }
                }
                // update total size and create document
                document.document_size = total_size;
                _repository.CreateDocument(document);

                // insert textual data into elastic search
                if (document.content_type.Split('/')[0] == "text")
                {
                    // setup elastic client
                    ElasticClient client = _elasticSearchService.SetupElasticSearchClient();

                    // get uploaded content and send it to elastic search
                    byte[] documentContent = new byte[total_size];
                    chunk_size = 4 * 1024 * 1024; // take 4MB as chunk
                    int offset = 0;
                    while (total_size > offset)
                    {
                        // check if offset will be bigger then document size, then take rest of document size
                        if (offset + chunk_size > total_size)
                            chunk_size = total_size % chunk_size;
                        documentContent = _repository.GetDocumentContentInChunks(document._id, offset, chunk_size);
                        offset += chunk_size;
                    }

                    // receive string content and index it to Elastic server
                    string? elasticDocumentContent = _elasticSearchService.UploadDocument(client, documentContent);
                    if (elasticDocumentContent != null)
                    {
                        ElasticDocument elasticDocument = new()
                        {
                            id = document._id,
                            content = elasticDocumentContent,
                            name = document.document_name,
                            path_url = document.path_url
                        };
                        client.IndexDocument(elasticDocument);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void StreamDocumentContent(string filePath, int documentSize, Guid documentId, Func<Guid?, int, int, byte[]> GetDocumentContentInChunksFunction)
        {
            using (FileStream stream = System.IO.File.Create(filePath))
            {
                int chunk_size = 4 * 1024 * 1024;
                int offset = 0;
                while (documentSize > offset)
                {
                    // check if offset will be bigger then document size, then take rest of document size
                    if (offset + chunk_size > documentSize)
                        chunk_size = documentSize % chunk_size;
                    stream.Write(GetDocumentContentInChunksFunction(documentId, offset, chunk_size));
                    offset += chunk_size;
                }
            }
        }
        #endregion
    }
}
