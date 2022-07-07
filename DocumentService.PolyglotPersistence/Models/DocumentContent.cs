using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DocumentService.PolyglotPersistence.Models
{
    public class DocumentContent
    {
        [Key]
        public Guid _id { get; set; }
        [BsonRepresentation(BsonType.Binary)]
        public byte[] content { get; set; }
        public string content_type { get; set; }
    }
}
