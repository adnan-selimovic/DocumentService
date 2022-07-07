using System.ComponentModel.DataAnnotations;

namespace DocumentService.PolyglotPersistence.Models
{
    public class DbBuffer
    {
        [Key]
        public byte[] substring { get; set; }
    }
}
