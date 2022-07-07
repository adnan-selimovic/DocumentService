using DocumentService.PolyglotPersistence.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentService.PolyglotPersistence.DBContext
{
    public class SqlContext : DbContext
    {
        private readonly string document_content_tableName = "document_content";
        private readonly string document_history_content_tableName = "document_history_content";

        public SqlContext()
        {
            // Method intentionally left empty.
        }

        public SqlContext(DbContextOptions<SqlContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Method intentionally left empty.
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Method intentionally left empty.
        }

        public string DocumentContentTableName
        {
            get { return document_content_tableName; }
        }
        public string DocumentHistoryContentTableName
        {
            get { return document_history_content_tableName; }
        }

        public DbSet<Document> document { get; set; }
        public DbSet<DocumentContent> document_content { get; set; }
        public DbSet<DocumentHistory> document_history { get; set; }
        public DbSet<DocumentHistoryContent> document_history_content { get; set; }
        public DbSet<Folder> folder { get; set; }
        public DbSet<DbBuffer> dbBuffer { get; set; }
        public DbSet<PermissionStore> permission_store { get; set; }
    }
}
