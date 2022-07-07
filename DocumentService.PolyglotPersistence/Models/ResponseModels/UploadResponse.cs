using DocumentService.PolyglotPersistence.Models.ErrorModels;

namespace DocumentService.PolyglotPersistence.Models.ResponseModels
{
    public class UploadResponse
    {
        public int numberOfUploads { get; set; } = 0;
        public int numberOfUpdates { get; set; } = 0;
        public int numberOfFailed { get; set; } = 0;
        public List<Document> uploadedDocuments { get; set; } = new List<Document>();
        public List<Document> updatedDocuments { get; set; } = new List<Document>();
        public List<ErrorResponse> failedDocuments { get; set; } = new List<ErrorResponse>();
    }
}
