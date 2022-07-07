using System.ComponentModel.DataAnnotations;

namespace DocumentService.PolyglotPersistence.Models
{
    public class Folder
    {
        [Key]
        public Guid _id { get; set; }
        public string folder_name { get; set; }
        public string path_url { get; set; }
        public Guid parent_folder_id { get; set; }
        public string created_by { get; set; }
        public DateTime created_date { get; set; } = DateTime.UtcNow;
    }
}
