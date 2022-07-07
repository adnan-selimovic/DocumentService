using DocumentService.PolyglotPersistence.IServices;
using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.ErrorModels;
using DocumentService.PolyglotPersistence.Models.PermissionModels;
using DocumentService.PolyglotPersistence.Models.RequestModel;
using DocumentService.PolyglotPersistence.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace DocumentService.PolyglotPersistence.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        private readonly IFolderServices _folderService;
        private readonly IPermissionServices _permissionServices;

        public FolderController(IFolderServices folderService, IPermissionServices permissionServices)
        {
            _folderService = folderService;
            _permissionServices = permissionServices;
        }

        /// <summary>
        /// The response body will include a listing of all documents and subfolders in the specified folder.
        /// It returns a JSON object containing two arrays: an array of type Folder[]; contaning all subfolders and an array of type Document[]; with a document list from the current folder.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetFolderTree([Required] string folderPath = "/")
        {
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            FolderDocument? foldersAndDocuments = _folderService.GetFoldersAndDocuments(folderPath, requestorId);
            if (foldersAndDocuments != null)
                return StatusCode(200, foldersAndDocuments);
            return StatusCode(400, ErrorType.PathDoesNotExist);
        }

        /// <summary>
        /// An additional web method checking specifically for the PUBLIC user in the permissions list.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("public")]
        public IActionResult GetPublicFolderTree([Required] string folderPath = "/")
        {
            FolderDocument? foldersAndDocuments = _folderService.GetFoldersAndDocuments(folderPath, AccessType.Public.type);
            if (foldersAndDocuments != null)
                return StatusCode(200, foldersAndDocuments);
            return StatusCode(400, ErrorType.PathDoesNotExist);
        }

        /// <summary>
        /// Search for folders and documents by name.
        /// Used to implement autocomplete (type-ahead search) when users start typing a folder or document name into a search box.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("SearchByPath")]
        public virtual IActionResult SearchAllByPath([FromQuery] SearchQuery query)
        {
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (query.searchTerm == "")
                query.searchTerm = "/";
            return StatusCode(200, _folderService.SearchByPath(query, requestorId));
        }

        /// <summary>
        /// Renames a folder.
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPatch("{folderId}/rename")]
        public IActionResult RenameFolder([Required][FromRoute] Guid folderId, string name)
        {
            Folder? folder = _folderService.GetFolderById(folderId);
            if (folder == null)
                return StatusCode(400, ErrorType.FolderDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, folder._id, PermissionType.Write.group))
                return StatusCode(403, ErrorType.WritePermission);

            Folder? renamedFolder = _folderService.RenameFolder(folder, name);
            if (renamedFolder != null)
                return StatusCode(200, renamedFolder);
            return StatusCode(400, ErrorType.NameAlreadyUsed);
        }

        /// <summary>
        /// Creates a new folder in the folder hierarchy.
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CreateFolder(string folderName, [Required] string folderPath = "/")
        {
            // check if folder path exists
            Folder? folder = _folderService.GetFolderByPath(folderPath);
            if (folder == null)
                return StatusCode(400, ErrorType.PathDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, folder._id, PermissionType.Write.group))
                return StatusCode(403, ErrorType.WritePermission);

            // check if name is already taken
            if (folderPath != "/") folderPath += "/";
            if (_folderService.GetFolderByPath(folderPath + $"{folderName}") != null)
                return StatusCode(400, ErrorType.NameAlreadyUsed);

            return StatusCode(200, _folderService.CreateFolder(folder, folderName, requestorId));
        }

        [HttpDelete]
        public IActionResult DeleteFolder([Required][FromQuery] Guid[] folderId)
        {
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            foreach (var id in folderId)
            {
                // check permissions
                if (!_permissionServices.CheckPermissions(requestorId, id, PermissionType.Write.group))
                    return StatusCode(403, ErrorType.DeletePermission);

                Folder? folder = _folderService.GetFolderById(id);
                if (folder == null)
                    continue;
                _folderService.DeleteFolder(folder, requestorId);
            }
            return NoContent();
        }
    }
}
