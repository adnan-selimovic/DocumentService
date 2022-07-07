using DocumentService.PolyglotPersistence.IServices;
using DocumentService.PolyglotPersistence.Models.ErrorModels;
using DocumentService.PolyglotPersistence.Models.PermissionModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocumentService.PolyglotPersistence.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IFolderServices _folderService;
        private readonly IPermissionServices _permissionServices;

        public PermissionController(IFolderServices folderService, IPermissionServices permissionServices)
        {
            _folderService = folderService;
            _permissionServices = permissionServices;
        }

        /// <summary>
        /// Get permissions by path.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="folderId"></param>
        /// <param name="parentFolderId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetPermission([FromQuery] string folderPath, [FromQuery] Guid? folderId, [FromQuery] Guid? parentFolderId)
        {
            // if root is given, find its _ids from database
            if (_folderService.GetFolderIds(folderPath, ref folderId, ref parentFolderId) == null)
                return StatusCode(400, ErrorType.FolderDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, folderId, PermissionType.Owner.group))
                return StatusCode(403, ErrorType.NotOwner);

            // get permissions for given path and exclude requestor permissions from it
            return StatusCode(200, _permissionServices.GetPathPermission(folderId, requestorId));
        }

        [HttpPost("accessType")]
        public IActionResult SetAccessType([FromBody] AccessTypeRequest accessType, [FromQuery] string folderPath, [FromQuery] Guid? folderId, [FromQuery] Guid? parentFolderId)
        {
            // if root is given, find its _ids from database
            if (_folderService.GetFolderIds(folderPath, ref folderId, ref parentFolderId) == null)
                return StatusCode(400, ErrorType.FolderDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, folderId, PermissionType.Owner.group))
                return StatusCode(403, ErrorType.NotOwner);

            // if request is to set path to private, remove all public access
            if (accessType.accessType == AccessType.Private.type)
            {
                _permissionServices.DeletePermission(AccessType.Public.type, folderId);
                return NoContent();
            }

            _permissionServices.CreateAccessTypePermission(accessType, folderId, parentFolderId, requestorId);
            return NoContent();
        }

        /// <summary>
        /// Creates a permission on a document/folder.
        /// </summary>
        /// <param name="userPermission"></param>
        /// <param name="folderPath"></param>
        /// <param name="folderId"></param>
        /// <param name="parentFolderId"></param>
        /// <returns></returns>
        // TODO change to "path" & "pathId"
        [HttpPost]
        public IActionResult CreatePermission([FromBody] UserPermissionRequest userPermission, [FromQuery] string folderPath, [FromQuery] Guid? folderId, [FromQuery] Guid? parentFolderId)
        {
            // if root is given, find its _ids from database
            if (_folderService.GetFolderIds(folderPath, ref folderId, ref parentFolderId) == null)
                return StatusCode(400, ErrorType.FolderDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, folderId, PermissionType.Owner.group))
                return StatusCode(403, ErrorType.NotOwner);

            _permissionServices.CreatePermission(userPermission, folderId, parentFolderId, requestorId);
            return NoContent();
        }

        /// <summary>
        /// Update permission.
        /// </summary>
        /// <param name="userPermission"></param>
        /// <param name="folderPath"></param>
        /// <param name="folderId"></param>
        /// <param name="parentFolderId"></param>
        /// <returns></returns>
        [HttpPatch]
        public IActionResult UpdatePermission([FromBody] UserPermissionRequest userPermission, [FromQuery] string folderPath, [FromQuery] Guid? folderId, [FromQuery] Guid? parentFolderId)
        {
            // if root is given, find its _ids from database
            if (_folderService.GetFolderIds(folderPath, ref folderId, ref parentFolderId) == null)
                return StatusCode(400, ErrorType.FolderDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, folderId, PermissionType.Owner.group))
                return StatusCode(403, ErrorType.NotOwner);

            _permissionServices.UpdatePermission(userPermission.userId, userPermission.permissions, folderId, requestorId);
            return NoContent();
        }

        /// <summary>
        /// Removes all permissions on a document/folder for a user or group.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="folderPath"></param>
        /// <param name="folderId"></param>
        /// <param name="parentFolderId"></param>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult DeletePermission([FromQuery] string userId, [FromQuery] string folderPath, [FromQuery] Guid? folderId, [FromQuery] Guid? parentFolderId)
        {
            // if root is given, find its _ids from database
            if (_folderService.GetFolderIds(folderPath, ref folderId, ref parentFolderId) == null)
                return StatusCode(400, ErrorType.FolderDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, folderId, PermissionType.Owner.group))
                return StatusCode(403, ErrorType.NotOwner);

            _permissionServices.DeletePermission(userId, folderId);
            return NoContent();
        }
    }
}
