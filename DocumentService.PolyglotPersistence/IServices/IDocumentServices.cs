using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.Elasticsearch;
using DocumentService.PolyglotPersistence.Models.RequestModel;
using DocumentService.PolyglotPersistence.Models.ResponseModels;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace DocumentService.PolyglotPersistence.IServices
{
    public interface IDocumentServices
    {
        // DocumentController services
        void CheckInDocument(ref Document document);
        void CheckOutDocument(ref Document document, string requestorId);
        string? GetPathOfStreamedDocument(string documentName, int documentSize, Guid documentId, int? version);
        IEnumerable<DocumentHistory> GetDocumentHistory(Guid documentId);
        void RenameDocument(ref Document document, string name);
        Document? UpdateDocument(MultipartSection section, Document document, ContentDispositionHeaderValue contentDisposition, string requestorId);
        Task<UploadResponse> UploadDocument(MultipartReader multipartReader, Folder folder, IEnumerable<Document> existingDocuments, string requestorId);
        void DeleteDocument(Document document);
        PageResponse<ElasticDocument> SearchDocument(SearchQuery query, string requestorId);
        List<HierarchyData>? GetPathHierarchy(string folderPath, string requestorId);

        // Helper services
        Document? GetDocument(Guid documentId);
        bool CheckDocumentName(Guid parentFolderId, string name);
        DocumentHistory? GetDocumentVersion(Guid documentId, int version);
    }
}
