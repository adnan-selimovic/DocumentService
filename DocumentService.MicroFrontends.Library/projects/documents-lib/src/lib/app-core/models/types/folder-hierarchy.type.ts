export type FolderHierarchy = {
  _id: string;
  name: string;
  isFolder: boolean;
  isExpanded?: boolean;
  path_url: string;
  childFolder: FolderHierarchy[] | null;
};
