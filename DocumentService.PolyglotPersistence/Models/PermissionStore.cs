using System.ComponentModel.DataAnnotations;

namespace DocumentService.PolyglotPersistence.Models
{
    public class PermissionStore
    {
        [Key]
        public Guid _id { get; set; }
        public string user_id { get; set; }
        public Guid? path_id { get; set; }
        public Guid? parent_path_id { get; set; }
        public string permission { get; set; }
        public string assigned_by { get; set; }
        public DateTime assigned_date { get; set; } = DateTime.UtcNow;
    }
}
