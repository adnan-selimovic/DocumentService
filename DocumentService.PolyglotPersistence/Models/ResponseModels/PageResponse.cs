namespace DocumentService.PolyglotPersistence.Models.ResponseModels
{
    public class PageResponse<T>
    {
        public long totalSize { get; set; }
        public IEnumerable<T> items { get; set; }
    }
}
