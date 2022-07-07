namespace DocumentService.PolyglotPersistence.Models.PermissionModels
{
    public class PermissionType
    {
        public string type { get; private set; }
        public IEnumerable<string> group { get; private set; }

        private PermissionType(string type, IEnumerable<string> group)
        {
            this.type = type;
            this.group = group;
        }

        // administrator
        public readonly static PermissionType Admin = new("admin", new List<string>());

        // admin
        public readonly static PermissionType All = new("all", new List<string>());
        public readonly static PermissionType Owner = new("owner", new List<string>() { "owner", All.type });
        // read
        public readonly static PermissionType Read = new("read", new List<string>() { "read", All.type, Owner.type });
        // write
        public readonly static PermissionType Write = new("write", new List<string>() { "write", All.type, Owner.type });
        // delete
        public readonly static PermissionType Delete = new("delete", new List<string>() { "delete", All.type, Owner.type });
    }
}
