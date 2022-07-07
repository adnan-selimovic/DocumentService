namespace DocumentService.PolyglotPersistence.Models.PermissionModels
{
    public class AccessTypeRequest
    {
        public string accessType { get; set; }
        public List<string> permissions { get; set; }
    }
}
