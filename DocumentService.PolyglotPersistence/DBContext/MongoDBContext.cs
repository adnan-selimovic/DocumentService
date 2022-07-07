using DocumentService.PolyglotPersistence.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DocumentService.PolyglotPersistence.DBContext
{
    public class MongoDBContext
    {
        public readonly IMongoDatabase mongoDatabase;

        public MongoDBContext(IOptions<DatabaseSettings> bookStoreDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                bookStoreDatabaseSettings.Value.ConnectionString);

            mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);
        }

        public IMongoCollection<Document> document
        {
            get
            {
                return mongoDatabase.GetCollection<Document>("document");
            }
        }

        public IMongoCollection<DocumentContent> document_content
        {
            get
            {
                return mongoDatabase.GetCollection<DocumentContent>("document_content");
            }
        }

        public IMongoCollection<DocumentHistory> document_history
        {
            get
            {
                return mongoDatabase.GetCollection<DocumentHistory>("document_history");
            }
        }

        public IMongoCollection<DocumentHistoryContent> document_history_content
        {
            get
            {
                return mongoDatabase.GetCollection<DocumentHistoryContent>("document_history_content");
            }
        }

        public IMongoCollection<Folder> folder
        {
            get
            {
                return mongoDatabase.GetCollection<Folder>("folder");
            }
        }

        public IMongoCollection<PermissionStore> permission_store
        {
            get
            {
                return mongoDatabase.GetCollection<PermissionStore>("permission_store");
            }
        }

    }
}
