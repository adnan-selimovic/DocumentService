using DocumentService.PolyglotPersistence.IRepositories;
using DocumentService.PolyglotPersistence.IServices;
using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.PermissionModels;

namespace DocumentService.PolyglotPersistence.Services
{
    public class PermissionServices : IPermissionServices
    {
        private readonly IRepository _repository;

        public PermissionServices(IRepository repository)
        {
            _repository = repository;
        }

        public bool CheckPermissions(string requestorId, Guid? pathId, IEnumerable<string> permissions) =>
            _repository.CheckRequestorPermissions(requestorId, pathId, permissions);

        public IEnumerable<UserPermissionRequest> GetPathPermission(Guid? pathId, string requestorId)
        {
            // get permissions for given path
            IEnumerable<PermissionStore> permissions = _repository.GetPathPermission(pathId);
            // distinct all users from permissions and create UserPermission model for each
            IEnumerable<string>? users = permissions.DistinctBy(x => x.user_id).Select(x => x.user_id);
            List<UserPermissionRequest> pathPermissions = new();
            foreach (string? user in users)
            {
                pathPermissions.Add(new UserPermissionRequest()
                {
                    userId = user,
                    permissions = permissions.Where(x => x.user_id == user).Select(x => x.permission)
                });
            }

            // exclude owner permissions
            return pathPermissions.Where(p => p.userId != requestorId);
        }
            

        public void CreateAccessTypePermission(AccessTypeRequest accessType, Guid? folderId, Guid? parentFolderId, string requestorId)
        {
            // create new list of permissions
            List<PermissionStore> permissions = new();
            // get existing permission, to check upon, and add new permissions
            IEnumerable<PermissionStore> existingPermissions = _repository.GetPathPermission(folderId);
            foreach (var permission in accessType.permissions)
            {
                // if permission exists, skip
                if (existingPermissions.Any(x => x.user_id == AccessType.Public.type && x.permission == permission))
                    continue;

                permissions.Add(new PermissionStore()
                {
                    _id = Guid.NewGuid(),
                    user_id = AccessType.Public.type,
                    permission = permission,
                    path_id = folderId,
                    parent_path_id = parentFolderId,
                    assigned_by = requestorId
                });
            }

            // if there are any permissions, add them
            if (permissions.Any())
                _repository.CreatePermission(permissions);
        }

        public void CreatePermission(UserPermissionRequest userPermission, Guid? folderId, Guid? parentFolderId, string requestorId)
        {
            // create new list of permissions
            List<PermissionStore> permissions = new();
            // get existing permission, to check upon, and add new permissions
            IEnumerable<PermissionStore> existingPermissions = _repository.GetPathPermission(folderId);
            foreach (var permission in userPermission.permissions)
            {
                // if permission exists, skip
                if (existingPermissions.Any(x => x.user_id == userPermission.userId && x.permission == permission))
                    continue;

                permissions.Add(new PermissionStore()
                {
                    _id = Guid.NewGuid(),
                    user_id = userPermission.userId,
                    permission = permission,
                    path_id = folderId,
                    parent_path_id = parentFolderId,
                    assigned_by = requestorId
                });
            }

            // if there are any permissions, add them
            if (permissions.Any())
                _repository.CreatePermission(permissions);
        }
            
        public void UpdatePermission(string userId, IEnumerable<string> permissions, Guid? pathId, string assignee) =>
            _repository.UpdatePermission(userId, permissions, pathId, assignee);
        public void DeletePermission(string userId, Guid? pathId) =>
            _repository.DeletePermission(AccessType.Public.type, pathId);
    }
}
