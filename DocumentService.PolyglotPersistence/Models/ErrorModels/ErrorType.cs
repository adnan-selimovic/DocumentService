using Newtonsoft.Json;

namespace DocumentService.PolyglotPersistence.Models.ErrorModels
{
    public class ErrorType
    {
        public int status { get; private set; }
        public string code { get; private set; }
        public string message { get; set; }

        private ErrorType(int status, string name, string description)
        {
            this.status = status;
            this.code = name;
            this.message = description;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        // check in/out enums
        public readonly static ErrorType CheckOutBySomeoneElse = new(400, "ERR_NOT_CHECKED_OUT", "Document not checked out");
        public readonly static ErrorType NotCheckedOut = new(400, "ERR_NOT_CHECKED_OUT", "Document not checked out");
        public readonly static ErrorType AlreadyCheckedIn = new(400, "ERR_ALREADY_CHECKED_IN", "Document already checked in");
        public readonly static ErrorType AlreadyCheckedOut = new(400, "ERR_ALREADY_CHECKED_OUT", "Document already checked out");
        public readonly static ErrorType FailedtoCheckIn = new(400, "ERR_FAILED_TO_CHECK_IN", "Failed to check in document");
        public readonly static ErrorType FailedToCheckOut = new(400, "ERR_FAILED_TO_CHECK_OUT", "Failed to check out document");

        // document enums
        public readonly static ErrorType FailedToUpdateDocument = new(400, "ERR_FAILED_UPDATE", "Document not updated");
        public readonly static ErrorType FailedToUploadDocument = new(400, "ERR_FAILED_UPLOAD", "Document not uploaded");
        public readonly static ErrorType DocumentHasNoContent = new(400, "ERR_NO_CONTENT", "Document has no content");
        public readonly static ErrorType DocumentDoesNotExist = new(400, "ERR_DOCUMENT_DOES_NOT_EXIST", "Document does not exist");
        public readonly static ErrorType DocumentVersionDoesNotExist = new(400, "ERR_DOCUMENT_VERSION_DOES_NOT_EXIST", "Document version does not exist");
        public readonly static ErrorType InvalidDocumentIdFormat = new(400, "ERR_INVALID_DOCUMENT_ID_FORMAT", "Invalid documentId format");
        public readonly static ErrorType DocumentStreamFailed = new(400, "ERR_DOCUMENT_STREAM_FAILED", "Document failed to stream");

        // folder enums
        public readonly static ErrorType FolderDoesNotExist = new(400, "ERR_FOLDER_DOES_NOT_EXIST", "Folder does not exist");
        public readonly static ErrorType InvalidFolderFormat = new(400, "ERR_INVALID_FOLDER_FORMAT", "Invalid folder format");

        // general enums
        public readonly static ErrorType NameAlreadyUsed = new(400, "ERR_NAME_TAKEN", "This destination already contains given name");
        public readonly static ErrorType PathDoesNotExist = new(400, "ERR_PATH_DOES_NOT_EXIST", "Path does not exist");

        // permission enums
        public readonly static ErrorType WritePermission = new(403, "ERR_WRITE_PERMISSION", "No permission to write");
        public readonly static ErrorType ReadPermission = new(403, "ERR_READ_PERMISSION", "No permission to read");
        public readonly static ErrorType DeletePermission = new(403, "ERR_DELETE_PERMISSION", "No permission to delete");
        public readonly static ErrorType NotOwner = new(403, "ERR_NOT_OWNER", "Not owner");

        // unhandled exceptions
        public readonly static ErrorType UserUnhandledException = new(500, "ERR_USER_UNHANDLED_EXCEPTION", "");
    }
}
