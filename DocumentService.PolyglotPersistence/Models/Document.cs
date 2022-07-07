using System.ComponentModel.DataAnnotations;

namespace DocumentService.PolyglotPersistence.Models
{
    public class Document
    {
        [Key]
        public Guid _id { get; set; }
        public string document_name { get; set; }
        public string content_type { get; set; }
        public int document_size { get; set; }
        public string path_url { get; set; }
        public string created_by { get; set; }
        public DateTime created_date { get; set; } = DateTime.UtcNow;
        public Guid folder_id { get; set; }
        public int version { get; set; } = 1;
        public string? checked_out_by { get; set; } = null;
        public DateTime? checked_out_date { get; set; } = null;
    }
}
