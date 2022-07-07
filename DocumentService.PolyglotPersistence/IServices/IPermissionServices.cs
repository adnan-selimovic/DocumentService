using DocumentService.PolyglotPersistence.Models.PermissionModels;

namespace DocumentService.PolyglotPersistence.IServices
{
    public interface IPermissionServices
    {
        bool CheckPermissions(string requestorId, Guid? pathId, IEnumerable<string> permissions);
        IEnumerable<UserPermissionRequest> GetPathPermission(Guid? pathId, string requestorId);
        void CreateAccessTypePermission(AccessTypeRequest accessType, Guid? folderId, Guid? parentFolderId, string requestorId);
        void CreatePermission(UserPermissionRequest userPermission, Guid? folderId, Guid? parentFolderId, string requestorId);
        void UpdatePermission(string userId, IEnumerable<string> permissions, Guid? pathId, string assignee);
        void DeletePermission(string userId, Guid? pathId);
    }
}
