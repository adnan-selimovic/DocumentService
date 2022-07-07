using System.ComponentModel.DataAnnotations;

namespace DocumentService.PolyglotPersistence.Models
{
    public class DocumentHistory
    {
        [Key]
        public Guid _id { get; set; }
        public Guid document_id { get; set; }
        public string document_name { get; set; }
        public string content_type { get; set; }
        public int document_size { get; set; }
        public string path_url { get; set; }
        public string created_by { get; set; }
        public DateTime? period_from { get; set; }
        public DateTime? period_to { get; set; }
        public int version { get; set; }
    }
}
