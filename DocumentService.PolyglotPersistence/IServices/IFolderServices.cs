using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.RequestModel;
using DocumentService.PolyglotPersistence.Models.ResponseModels;

namespace DocumentService.PolyglotPersistence.IServices
{
    public interface IFolderServices
    {
        // FolderController services
        FolderDocument? GetFoldersAndDocuments(string path, string requestorId);
        PageResponse<SearchPath> SearchByPath(SearchQuery query, string requestorId);
        Folder? RenameFolder(Folder folder, string newName);
        Folder CreateFolder(Folder folder, string folderName, string requestorId);
        void DeleteFolder(Folder folder, string requestorId);

        // Helper services
        Folder? GetFolderByPath(string folderPath);
        Folder? GetFolderById(Guid folderId);
        Folder? GetFolderIds(string folderPath, ref Guid? folderId, ref Guid? parentFolderId);
    }
}
