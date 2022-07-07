import {
  FolderPath,
  FolderPreviewType,
  RDocuments,
  PaginationResponse,
  TDocument,
  FolderHierarchy,
  FolderOptions,
  SearchDocumentType,
  SearchFolderHierarchyModel,
  UserPermissionType,
} from '../../../app-core/models';

export class DocumentsStateModel {
  loading!: boolean;
  error: any;
  options!: FolderOptions;

  folderPath!: FolderPath;
  rootFolder!: RDocuments | null;
  publicFolder!: RDocuments | null; // PUBLIC USERS
  folderPathPermissions!: UserPermissionType[] | null;
  selectedDocument!: TDocument | null;

  foldersTree!: FolderHierarchy[] | null;

  folderSearch!: PaginationResponse<SearchFolderHierarchyModel> | null;
  documentSearch!: PaginationResponse<SearchDocumentType> | null;
  searchInProgress!: boolean;
}

export const defaultState: DocumentsStateModel = {
  loading: false,
  error: null,
  options: {
    previewType: FolderPreviewType.FOLDER,
  },
  folderPath: { folderPath: '/', folderPathArr: ['/'] } as FolderPath,
  rootFolder: null,
  publicFolder: null, // PUBLIC USERS
  folderPathPermissions: null,
  selectedDocument: null,
  foldersTree: null,
  folderSearch: null,
  documentSearch: null,
  searchInProgress: false,
};
