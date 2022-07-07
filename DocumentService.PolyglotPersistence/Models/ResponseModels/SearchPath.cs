namespace DocumentService.PolyglotPersistence.Models.ResponseModels
{
    public class SearchPath
    {
        public Guid _id { get; set; }
        public string name { get; set; }
        public string path_url { get; set; }
        public bool is_folder { get; set; }
    }
}
