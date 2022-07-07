using Nest;

namespace DocumentService.PolyglotPersistence.IServices
{
    public interface IElasticSearchServices
    {
        ElasticClient SetupElasticSearchClient();
        string? UploadDocument(ElasticClient client, byte[] buffer);
    }
}
