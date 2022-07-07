using DocumentService.PolyglotPersistence.IServices;
using DocumentService.PolyglotPersistence.Models.Elasticsearch;
using Elasticsearch.Net;
using Nest;

namespace DocumentService.PolyglotPersistence.Services
{
    public class ElasticSearchServices : IElasticSearchServices
    {
        // setup ElasticSearch client
        public ElasticClient SetupElasticSearchClient()
        {
            string connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ElasticSearch").GetSection("ConnectionString").Value;
            string certificate = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ElasticSearch").GetSection("Certificate").Value;
            string username = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ElasticSearch").GetSection("Username").Value;
            string password = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ElasticSearch").GetSection("Password").Value;

            ConnectionSettings settings = new ConnectionSettings(new Uri(connectionString)).BasicAuthentication(username, password).CertificateFingerprint(certificate).DefaultIndex("search");

            return new ElasticClient(settings);
        }

        // upload document to elastic search server
        public string? UploadDocument(ElasticClient client, byte[] buffer)
        {
            Id attach = "attachment_search";

            // create pipeline
            client.Ingest.PutPipeline(new PutPipelineRequest(attach)
            {
                Description = "Attachment pipeline test",
                Processors = new List<IProcessor>
                                    {
                                        new AttachmentProcessor
                                        {
                                            Field = "data"
                                        }
                                    }
            });

            // convert byte[] to base64
            string docContent = Convert.ToBase64String(buffer);
            ElasticSource elasticSource = new() { data = docContent };

            // initialize pipeline and format that for it to work
            ISimulatePipelineDocument simulatePipelineDocuments = new SimulatePipelineDocument();
            simulatePipelineDocuments.Id = "attachment_search";
            simulatePipelineDocuments.Index = "search";
            simulatePipelineDocuments.Source = elasticSource;
            ISimulatePipelineRequest simulatePipelineRequest = new SimulatePipelineRequest("attachment_search");
            simulatePipelineRequest.Documents = new[] { simulatePipelineDocuments };

            // activate pipeline and return content in string
            SimulatePipelineResponse elasticResult = client.Ingest.SimulatePipeline(simulatePipelineRequest);
            if (elasticResult.IsValid)
            {
                var elasticDocumentContent = elasticResult.Documents.First().Document.Source.As(typeof(Source)) as Source;
                if (elasticDocumentContent == null)
                    return "";
                return elasticDocumentContent.attachment.content;
            }
            return null;
        }
    }
}
