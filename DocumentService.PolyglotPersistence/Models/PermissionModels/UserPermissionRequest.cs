using System.ComponentModel.DataAnnotations;

namespace DocumentService.PolyglotPersistence.Models.PermissionModels
{
    public class UserPermissionRequest
    {
        public string userId { get; set; }
        public IEnumerable<string> permissions { get; set; }
        public bool isGroup { get; set; }
    }
}
