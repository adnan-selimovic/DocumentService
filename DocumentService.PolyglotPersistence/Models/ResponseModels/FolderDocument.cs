using DocumentService.PolyglotPersistence.Models.PermissionModels;

namespace DocumentService.PolyglotPersistence.Models.ResponseModels
{
    public class FolderDocument
    {
        public FolderDocument(Folder currentFolder, IEnumerable<Folder> subFolders, IEnumerable<Document> documents, IEnumerable<PermissionStore> permissionStore, bool getAll)
        {
            // if request is sent by service (getAll == true), then return all folders/documents
            if (getAll)
            {
                this.folders = subFolders;
                this.documents = documents;
            }
            else
            {
                this.folders = subFolders.Where(f => permissionStore.Any(p => p.path_id == f._id));
                this.documents = documents.Where(f => permissionStore.Any(p => p.path_id == f._id));
            }
            foreach (var permission in permissionStore.Where(p => p.path_id == currentFolder._id)) // check permissions that corespond to current path, then distinct them
                this.permissions.Add(permission.permission);
            this.permissions = this.permissions.Distinct().ToList();
            this.currentFolder = (currentFolder.parent_folder_id == Guid.Empty) ? null : currentFolder; // if current folder is root, dont show it (i.e. send null)
            this.accessType = permissionStore.Any(ps => ps.path_id == currentFolder._id && ps.user_id == AccessType.Public.type) ? AccessType.Public.type : AccessType.Private.type; // check if folder is public or not
        }

        public FolderDocument(Document currentDocument, IEnumerable<Folder> subFolders, IEnumerable<Document> documents, IEnumerable<PermissionStore> permissionStore, bool getAll)
        {
            // if request is sent by service (getAll == true), then return all folders/documents
            if (getAll)
            {
                this.folders = subFolders;
                this.documents = documents;
            }
            else
            {
                this.folders = subFolders.Where(f => permissionStore.Any(p => p.path_id == f._id));
                this.documents = documents.Where(f => permissionStore.Any(p => p.path_id == f._id));
            }
            foreach (var permission in permissionStore.Where(p => p.path_id == currentDocument._id)) // check permissions that corespond to current path, then distinct them
                this.permissions.Add(permission.permission);
            this.permissions = this.permissions.Distinct().ToList();
            this.currentFolder = currentDocument;
            this.accessType = permissionStore.Any(ps => ps.path_id == currentDocument._id && ps.user_id == AccessType.Public.type) ? AccessType.Public.type : AccessType.Private.type; // check if document is public or not
        }

        public object? currentFolder { get; set; }
        public IEnumerable<Folder> folders { get; set; }
        public IEnumerable<Document> documents { get; set; }
        public List<string> permissions { get; set; } = new List<string>();
        public string accessType { get; set; }
    }
}
