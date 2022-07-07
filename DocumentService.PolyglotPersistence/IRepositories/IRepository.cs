using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.ResponseModels;

namespace DocumentService.PolyglotPersistence.IRepositories
{
    public interface IRepository
    {
        // document services
        Document? GetDocument(Guid documentId);
        bool CheckDocumentName(Guid parentFolderId, string name);
        void CreateDocument(Document document);
        void CreateDocumentVersion(DocumentHistory documentVersion);
        void RenameDocument(Document document, string name);
        void DeleteDocument(Document document, bool deleteHistory);
        void RestoreDocument(Document document);

        // document content servies
        byte[] GetDocumentContentInChunks(Guid? documentId, int offset, int chunk_size);
        void CreateDocumentContent(DocumentContent documentContent);
        int CreateDocumentContentInChunks(Guid documentId, byte[] buffer, ref HashSet<string> blocklist);

        // document history services
        IEnumerable<DocumentHistory> GetDocumentHistory(Guid? documentId);
        DocumentHistory? GetDocumentByVersionHistory(Guid? documentId, int version);
        byte[] GetDocumentContentVersionInChunks(Guid? documentId, int offset, int chunk_size);

        // check in/check out services
        void CheckInDocument(Document document);
        void CheckOutDocument(Document document, string requestorId);

        // folder services
        FolderDocument? GetAllFromPath(string folderPath, string requestorId);
        List<HierarchyData>? GetHierarchy(string folderPath, string requestorId);
        Folder? GetFolderByName(string folderName);
        Folder? GetFolderByPath(string folderPath);
        Folder? GetFolderById(Guid folderId);
        bool CheckFolderName(Folder folder, string name);
        Folder? RenameFolder(Folder folder, string name);
        void RenamePath(Folder folder, string newFolderPath, string name);
        Folder CreateFolder(Folder folder);
        void DeleteFolder(Folder folder);

        // search services
        List<SearchPath> SearchByPath(int page, int pageSize, string searchTerm, string requestorId);

        // permission services
        bool CheckRequestorPermissions(string requestorId, Guid? pathId, IEnumerable<string> permissions);
        List<PermissionStore> GetUserPermissionsByParentPathId(string userId, Guid parentPathId);
        IEnumerable<PermissionStore> GetUserPermissionByPathId(string userId, Guid pathId);
        IEnumerable<PermissionStore> GetPathPermission(Guid? pathId);
        void CreatePermission(List<PermissionStore> permissionStore);
        void UpdatePermission(string userId, IEnumerable<string> permissions, Guid? pathId, string assignee);
        void DeletePermission(string userId, Guid? pathId);
    }
}
