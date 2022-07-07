using DocumentService.PolyglotPersistence.IRepositories;
using DocumentService.PolyglotPersistence.IServices;
using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.PermissionModels;
using DocumentService.PolyglotPersistence.Models.RequestModel;
using DocumentService.PolyglotPersistence.Models.ResponseModels;

namespace DocumentService.PolyglotPersistence.Services
{
    public class FolderServices : IFolderServices
    {
        private readonly IRepository _repository;

        public FolderServices(IRepository repository)
        {
            _repository = repository;
        }

        #region // FolderController services
        public FolderDocument? GetFoldersAndDocuments(string path, string requestorId) =>
            _repository.GetAllFromPath(path, requestorId);

        public PageResponse<SearchPath> SearchByPath(SearchQuery query, string requestorId)
        {
            // get all paths from folders and documents
            List<SearchPath> paths = _repository.SearchByPath(query.page, query.pageSize, query.searchTerm, requestorId);
            // sort by name and return requested size
            paths.Sort((x, y) => string.Compare(x.path_url, y.path_url));
            return new PageResponse<SearchPath>()
            {
                totalSize = paths.Count,
                items = paths.Where(x => !_repository.CheckRequestorPermissions(requestorId, x._id, PermissionType.Read.group)).Skip((query.page - 1) * query.pageSize).Take(query.pageSize)
            };
        }
            

        public Folder? RenameFolder(Folder folder, string newName)
        {
            // check if folder names already exists
            if (_repository.CheckFolderName(folder, newName))
                return null;

            // rename all subPaths of targeted folder
            _repository.RenamePath(folder, folder.path_url[..^(folder.folder_name.Length + 1)], newName);
            // rename target folder
            return _repository.RenameFolder(folder, newName);
        }
            

        public Folder CreateFolder(Folder folder, string folderName, string requestorId)
        {
            Folder newFolder = new()
            {
                _id = Guid.NewGuid(),
                folder_name = folderName,
                parent_folder_id = folder._id,
                path_url = (folder.parent_folder_id == Guid.Empty) ? folder.path_url + folderName : folder.path_url + "/" + folderName,
                created_by = requestorId,
                created_date = DateTime.UtcNow
            };

            List<PermissionStore> permissions = new List<PermissionStore>()
            {
                new PermissionStore()
                {
                    _id = Guid.NewGuid(),
                    user_id = requestorId,
                    permission = PermissionType.Owner.type,
                    path_id = newFolder._id,
                    parent_path_id = newFolder.parent_folder_id,
                    assigned_by = requestorId
                }
            };
            _repository.CreatePermission(permissions);

            return _repository.CreateFolder(newFolder);
        }
        public void DeleteFolder(Folder folder, string requestorId)
        {
            FolderDocument? foldersAndDocuments = _repository.GetAllFromPath(folder.path_url, requestorId);
            if (foldersAndDocuments == null)
                return;

            foreach (Document document in foldersAndDocuments.documents)
                _repository.DeleteDocument(document, true); // delete documents
            foreach (Folder subFolder in foldersAndDocuments.folders)
                DeleteFolder(subFolder, requestorId); // recoursion

            _repository.DeleteFolder(folder); // delete folders
        }
        # endregion

        #region // Helper services
        public Folder? GetFolderByPath(string folderPath) =>
            _repository.GetFolderByPath(folderPath);

        public Folder? GetFolderById(Guid folderId) =>
            _repository.GetFolderById(folderId);

        public Folder? GetFolderIds(string folderPath, ref Guid? folderId, ref Guid? parentFolderId)
        {
            if (folderId == null || parentFolderId == null)
            {
                Folder? folder = _repository.GetFolderByPath(folderPath);
                if (folder == null)
                    return null;
                folderId = folder._id;
                parentFolderId = folder.parent_folder_id;
            }
            return new();
        }
        #endregion
    }
}
