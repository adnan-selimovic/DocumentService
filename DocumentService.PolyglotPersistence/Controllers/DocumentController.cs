using DocumentService.PolyglotPersistence.IServices;
using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.ErrorModels;
using DocumentService.PolyglotPersistence.Models.PermissionModels;
using DocumentService.PolyglotPersistence.Models.RequestModel;
using DocumentService.PolyglotPersistence.Models.ResponseModels;
using DocumentService.PolyglotPersistence.Services.HelperServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace DocumentService.PolyglotPersistence.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentServices _documentService;
        private readonly IFolderServices _folderService;
        private readonly IPermissionServices _permissionServices;

        private static readonly FormOptions _defaultFormOptions = new();

        public DocumentController(IDocumentServices documentService, IFolderServices folderService, IPermissionServices permissionServices)
        {
            _documentService = documentService;
            _folderService = folderService;
            _permissionServices = permissionServices;
        }

        /// <summary>
        /// The check-out operation prevents other users from editing the document and changes from being visible until the document is checked-in.
        /// </summary>
        /// <param name="documentID"></param>
        /// <remarks>
        /// This example checks out a document identified by `{documentID}`:
        /// ```
        ///   POST /api/Document/81a130d2-502f-4cf1-a376-63edeb000e9f/checkout HTTP/1.1
        ///   Host: localhost:7107
        /// ```
        /// The following shows the response:
        /// ```
        ///   HTTP/1.1 200 OK
        /// ```
        /// </remarks>
        /// <returns code="200">If successful, the API call returns a 204 No content status code.</returns>
        /// <response code="400">Document does not exist or unable to check-out.</response>
        /// <response code="403">Permission denied.</response>
        [HttpPost("{documentID}/checkout")]
        public virtual IActionResult Checkout([FromRoute][Required] Guid documentID)
        {
            // check if document exists
            Document? document = _documentService.GetDocument(documentID);
            if (document == null)
                return StatusCode(400, ErrorType.DocumentDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, document._id, PermissionType.Write.group))
                return StatusCode(403, ErrorType.WritePermission);

            // check if document is already checked out
            if (document.checked_out_by != null)
                return StatusCode(400, ErrorType.AlreadyCheckedOut);

            _documentService.CheckOutDocument(ref document, requestorId);
            return StatusCode(200, document);
        }

        /// <summary>
        /// Downloads the document with the given ID.
        /// The path parameter `documentId` is part of the operation's URL. Note that you can also download a document when you know its full path; for that, see the `/Folder/{folderPath}` endpoint.
        /// </summary>
        /// <param name="documentId">The ID (GUID) of the document. The GUID format is dictated by the `Guid.Parse(String)` method https://docs.microsoft.com/en-us/dotnet/api/system.guid.parse.</param>
        /// <remarks>
        /// The following HTTP GET request gets document content:
        /// ```
        ///   GET /api/Document/81a130d2-502f-4cf1-a376-63edeb000e9f HTTP/1.1
        ///   Host: localhost:7107
        /// ```
        /// </remarks>
        /// <returns code="200">If successful, the API call returns a 200 OK status code.</returns>
        /// <response code="400">Document does not exist or document streaming error occurred.</response>
        /// <response code="403">Permission denied.</response>
        [HttpGet("{documentId}")]
        public virtual IActionResult GetDocumentById([FromRoute][Required] Guid documentId)
        {
            Document? document = _documentService.GetDocument(documentId);
            if (document == null)
                return StatusCode(400, ErrorType.DocumentDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, document._id, PermissionType.Read.group))
                return StatusCode(403, ErrorType.ReadPermission);

            string? streamPath = _documentService.GetPathOfStreamedDocument(document.document_name, document.document_size, document._id, null);
            if (streamPath != null)
                return PhysicalFile(streamPath, document.content_type);
            return StatusCode(400, ErrorType.DocumentStreamFailed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        /// <remarks>
        /// This example checks in a document identified by `{documentID}`:
        /// ```
        ///   POST /api/Document/81a130d2-502f-4cf1-a376-63edeb000e9f/checkin HTTP/1.1
        ///   Host: localhost:7107
        /// ```
        /// The following shows the response:
        /// ```
        ///   HTTP/1.1 200 OK
        /// ```
        /// </remarks>
        /// <response code="200">If successful, the API call returns a 200 OK status code.</response>
        /// <response code="400">Document does not exist or unable to check-in.</response>
        /// <response code="403">Permission denied.</response>
        [HttpPost("{documentID}/checkin")]
        public virtual IActionResult Checkin([FromRoute][Required] Guid documentID)
        {
            // check if document exists
            Document? document = _documentService.GetDocument(documentID);
            if (document == null)
                return StatusCode(400, ErrorType.DocumentDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, document._id, PermissionType.Write.group))
                return StatusCode(403, ErrorType.WritePermission);

            // check if document is already checked in
            if (document.checked_out_by != requestorId)
                return StatusCode(400, (document.checked_out_by == null) ? ErrorType.NotCheckedOut : ErrorType.CheckOutBySomeoneElse);

            _documentService.CheckInDocument(ref document);
            return StatusCode(200, document);
        }

        /// <summary>
        /// An additional web method checking specifically for the PUBLIC user in the permissions list.
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns code="200">If successful, the API call returns a 200 OK status code.</returns>
        /// <response code="400">Document does not exist or document streaming error occurred.</response>
        /// <response code="403">Permission denied.</response>
        [AllowAnonymous]
        [HttpGet("{documentId}/public")]
        public virtual IActionResult GetPublicDocumentById([FromRoute][Required] Guid documentId)
        {
            Document? document = _documentService.GetDocument(documentId);
            if (document == null)
                return StatusCode(400, ErrorType.DocumentDoesNotExist);

            // check permissions
            string? requestorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // remove this if token is sorted at frontend TODO
            if (requestorId == null)
                requestorId = AccessType.Public.type;
            if (!_permissionServices.CheckPermissions(requestorId, document._id, PermissionType.Read.group))
                return StatusCode(403, ErrorType.ReadPermission);

            string? streamPath = _documentService.GetPathOfStreamedDocument(document.document_name, document.document_size, document._id, null);
            if (streamPath != null)
                return PhysicalFile(streamPath, document.content_type);
            return StatusCode(400, ErrorType.DocumentStreamFailed);
        }

        /// <summary>
        /// You can use a WYSIWYG HTML editor as the document authoring tool to modify text-based documents without downloading.
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns code="200">If successful, the API call returns a 200 OK status code.</returns>
        /// <response code="400">Document does not exist or document streaming error occurred.</response>
        /// <response code="403">Permission denied.</response>
        [HttpGet("{documentId}/text")]
        public virtual IActionResult GetDocumentTextById([FromRoute][Required] Guid documentId)
        {
            Document? document = _documentService.GetDocument(documentId);
            if (document == null)
                return StatusCode(400, ErrorType.DocumentDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, document._id, PermissionType.Read.group))
                return StatusCode(403, ErrorType.ReadPermission);

            string? documentPath = _documentService.GetPathOfStreamedDocument(document.document_name, document.document_size, document._id, null);
            if (documentPath == null)
                return StatusCode(400, ErrorType.DocumentStreamFailed);

            return StatusCode(200, new DocumentText()
            {
                data = System.IO.File.ReadAllText(documentPath)
            });
        }

        /// <summary>
        /// Returns a list of all subfolders and documents in the specified folder.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns code="200">If successful, the API call returns a 200 OK status code.</returns>
        /// <response code="400">The specified path does not exist.</response>
        [HttpGet("hierarchy")]
        public IActionResult GetHierarchy([Required] string folderPath = "/")
        {
            // get identity user from token
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            List<HierarchyData>? hierarchy = _documentService.GetPathHierarchy(folderPath, requestorId);
            if (hierarchy != null)
                return StatusCode(200, hierarchy);
            return StatusCode(400, ErrorType.PathDoesNotExist);
        }

        /// <summary>
        /// Gets the history of the document. Check-out and check-in are the methods by which users can control when a new version is created.
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="version"></param>
        /// <response code="400">Indicates that the document does not have multiple versions or document streaming error occurred.</response>
        [HttpGet("{documentId}/history")]
        public virtual IActionResult GetDocumentHistory([FromRoute][Required] Guid documentId, int version = 0)
        {
            // check if document exists (passing 1 as version, to check if document history exists)
            DocumentHistory? document = _documentService.GetDocumentVersion(documentId, 1);
            if (document == null)
                return StatusCode(400, ErrorType.DocumentDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, documentId, PermissionType.Read.group))
                return StatusCode(403, ErrorType.ReadPermission);

            // if version is not provided (is 0) then return all versions of required document and their details
            if (version == 0)
                return StatusCode(200, _documentService.GetDocumentHistory(documentId));

            // if version is selected, then return information of given document version
            document = _documentService.GetDocumentVersion(documentId, version);
            if (document == null)
                return StatusCode(400, ErrorType.DocumentVersionDoesNotExist);

            string? streamPath = _documentService.GetPathOfStreamedDocument(document.document_name, document.document_size, document._id, document.version);
            if (streamPath != null)
                return PhysicalFile(streamPath, document.content_type);
            return StatusCode(400, ErrorType.DocumentStreamFailed);
        }

        /// <summary>
        /// Documents are indexed and searchable for specific content using Elasticsearch.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("SearchByContent")]
        public virtual IActionResult SearchByName([FromQuery] SearchQuery query)
        {
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            return StatusCode(200, _documentService.SearchDocument(query, requestorId));
        }

        /// <summary>
        /// The rename document operation.
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPatch("{documentId}/rename")]
        public virtual IActionResult RenameDocument([Required][FromRoute] Guid documentId, string name)
        {
            Document? document = _documentService.GetDocument(documentId);
            if (document == null)
                return StatusCode(400, ErrorType.DocumentDoesNotExist);

            // check permissions
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!_permissionServices.CheckPermissions(requestorId, documentId, PermissionType.Read.group))
                return StatusCode(403, ErrorType.WritePermission);

            // rename document
            _documentService.RenameDocument(ref document, name);
            return StatusCode(200, document);
        }

        /// <summary>
        /// Updates an existing document contents or metadata.
        /// </summary>
        /// <returns></returns>
        [DisableRequestSizeLimit]
        [HttpPut]
        public async Task<IActionResult> UpdateDocument()
        {
            // get requestor Id from token
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            #region // perform check for Multipart request
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed");
                // Log error
                return StatusCode(400, ModelState);
            }
            string boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            MultipartReader multipartReader = new(boundary, HttpContext.Request.Body);
            #endregion

            // folder path out of form
            MultipartSection? section = await multipartReader.ReadNextSectionAsync();
            if (section == null)
                return StatusCode(400, ErrorType.InvalidDocumentIdFormat);
            _ = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var folderPathDisposition);
            StreamReader documentIdStream = new(section.Body);
            // check if given Id can be converted to uuid
            if (!Guid.TryParse(documentIdStream.ReadToEnd(), out var documentId))
                return StatusCode(400, ErrorType.InvalidDocumentIdFormat);

            Document? document = _documentService.GetDocument(documentId);
            if (document == null)
                return StatusCode(400, ErrorType.DocumentDoesNotExist);

            // check if user has enough permissions to update document
            if (!_permissionServices.CheckPermissions(requestorId, document._id, PermissionType.Write.group))
                return StatusCode(403, ErrorType.WritePermission);

            // check if document is checked out or if it checked out by someone else
            if (document.checked_out_by != requestorId)
                return StatusCode(400, (document.checked_out_by == null) ? ErrorType.NotCheckedOut : ErrorType.AlreadyCheckedOut);

            // read file section
            section = await multipartReader.ReadNextSectionAsync();
            if (section == null)
                return StatusCode(400, ErrorType.DocumentHasNoContent);
            // get header information
            _ = ContentDispositionHeaderValue.TryParse(
                   section.ContentDisposition, out var contentDisposition);

            // check if header is empty
            if (contentDisposition == null)
                return StatusCode(400, ErrorType.FailedToUpdateDocument);

            // check if name is already used
            if (contentDisposition.FileName.Value != document.document_name && _documentService.CheckDocumentName(document.folder_id, contentDisposition.FileName.Value))
                return StatusCode(400, ErrorType.NameAlreadyUsed);

            Document? updateDocument = _documentService.UpdateDocument(section, document, contentDisposition, requestorId);
            if (updateDocument == null)
                return StatusCode(400, ErrorType.FailedToUpdateDocument);
            return StatusCode(200, updateDocument);
        }

        /// <summary>
        /// Multiple file upload supporting "chunk transferring" to the server.
        /// </summary>
        /// <returns></returns>
        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> UploadDocument()
        {
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            #region // perform check for Multipart request
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed");
                // Log error
                return BadRequest(ModelState);
            }
            string boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            MultipartReader multipartReader = new(boundary, HttpContext.Request.Body);
            #endregion

            #region // take folder path from form and check if it exists
            // folder path out of form
            MultipartSection? section = await multipartReader.ReadNextSectionAsync();
            if (section == null)
                return StatusCode(400, ErrorType.InvalidFolderFormat);
            _ = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var folderPathDisposition);
            StreamReader folderStream = new(section.Body);
            string folderPath = folderStream.ReadToEnd();

            // check if folder exists 
            Folder? folder = _folderService.GetFolderByPath(folderPath);
            if (folder == null)
                return StatusCode(400, ErrorType.FolderDoesNotExist);

            // check if user has enough permissions to upload document
            if (!_permissionServices.CheckPermissions(requestorId, folder._id, PermissionType.Write.group))
                return StatusCode(403, ErrorType.WritePermission);

            FolderDocument? foldersAndDocuments = _folderService.GetFoldersAndDocuments(folderPath, PermissionType.Admin.type);
            if (foldersAndDocuments == null)
                return StatusCode(400, ErrorType.FolderDoesNotExist);
            IEnumerable<Document> existingDocuments = foldersAndDocuments.documents;
            #endregion

            return StatusCode(200, await _documentService.UploadDocument(multipartReader, folder, existingDocuments, requestorId));
        }

        /// <summary>
        /// Delete a document by the `documentId`.
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpDelete]
        public virtual IActionResult DeleteDocument([FromQuery] Guid[] documentId)
        {
            string requestorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            foreach (var id in documentId)
            {
                Document? document = _documentService.GetDocument(id);
                if (document == null)
                    return StatusCode(400, ErrorType.DocumentDoesNotExist);

                // check permissions
                if (!_permissionServices.CheckPermissions(requestorId, id, PermissionType.Delete.group))
                    return StatusCode(403, ErrorType.DeletePermission);

                _documentService.DeleteDocument(document);
            }
            return NoContent();
        }
    }
}
