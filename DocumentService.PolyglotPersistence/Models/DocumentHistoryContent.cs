using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DocumentService.PolyglotPersistence.Models
{
    public class DocumentHistoryContent
    {
        [Key]
        public Guid _id { get; set; }
        public Guid document_id { get; set; }
        public byte[] content { get; set; }
        public string content_type { get; set; }
    }
}
