using System.ComponentModel.DataAnnotations;

namespace DocumentService.PolyglotPersistence.Models.RequestModel
{
    public class SearchQuery
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 25;
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string searchTerm { get; set; }
    }
}
