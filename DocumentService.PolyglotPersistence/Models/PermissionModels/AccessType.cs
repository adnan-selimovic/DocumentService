namespace DocumentService.PolyglotPersistence.Models.PermissionModels
{
    public class AccessType
    {
        public string type { get; private set; }

        private AccessType(string type)
        {
            this.type = type;
        }

        // public
        public readonly static AccessType Public = new("public");
        // private
        public readonly static AccessType Private = new("private");
    }
}
