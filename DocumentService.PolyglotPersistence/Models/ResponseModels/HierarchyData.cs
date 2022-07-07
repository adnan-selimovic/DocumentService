namespace DocumentService.PolyglotPersistence.Models.ResponseModels
{
    public class HierarchyData
    {
        public HierarchyData()
        {

        }
        public HierarchyData(Folder folder, bool isExpanded)
        {
            this._id = folder._id;
            this.name = folder.folder_name;
            this.path_url = folder.path_url;
            this.isExpanded = isExpanded;
            this.isFolder = true;
        }

        public HierarchyData(Document document)
        {
            this._id = document._id;
            this.name = document.document_name;
            this.path_url = document.path_url;
            this.isExpanded = false;
            this.isFolder = false;
        }

        public Guid _id { get; set; }
        public string name { get; set; }
        public string path_url { get; set; }
        public bool isExpanded { get; set; }
        public bool isFolder { get; set; }
        public List<HierarchyData> childFolder { get; set; }
    }
}
