namespace DocumentService.PolyglotPersistence.Models.Elasticsearch
{
    public class Source
    {
        public string data { get; set; }
        public Attachment attachment { get; set; }
        public Doc doc { get; set; }
        public List<object> processor_results { get; set; }
    }

    public class Ingest
    {
        public DateTime timestamp { get; set; }
    }

    public class Attachment
    {
        public DateTime date { get; set; }
        public string content_type { get; set; }
        public string author { get; set; }
        public string language { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public int content_length { get; set; }
    }

    public class Doc
    {
        public string _id { get; set; }
        public string _index { get; set; }
        public Ingest _ingest { get; set; }
        public Source _source { get; set; }
        public string _type { get; set; }
    }
}
