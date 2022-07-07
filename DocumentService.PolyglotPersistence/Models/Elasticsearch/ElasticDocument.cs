namespace DocumentService.PolyglotPersistence.Models.Elasticsearch
{
    public class ElasticDocument
    {
        public Guid id { get; set; }
        public string content { get; set; }
        public string path_url { get; set; }
        public string name { get; set; }
    }
}
