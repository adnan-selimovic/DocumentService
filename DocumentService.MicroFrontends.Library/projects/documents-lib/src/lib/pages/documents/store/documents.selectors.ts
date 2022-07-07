import { Selector } from '@ngxs/store';
import {
  FolderPath,
  RDocuments,
  PaginationResponse,
  TDocument,
  FolderOptions,
  TFolder,
  SearchDocumentType,
  SearchFolderHierarchyModel,
  UserPermissionType,
  UserPermissionsClass,
} from '../../../app-core/models';
import { DocumentAccessTypeEnum } from '../../../app-core/models/enums/document-access-type.enum';
import { DocumentsStateModel } from './documents-state.model';
import { DocumentsState } from './documents.state';

export class DocumentsSelectors {
  @Selector([DocumentsState])
  public static getError(state: DocumentsStateModel): any {
    return state.error;
  }
  @Selector([DocumentsState])
  public static getFolderOptions(state: DocumentsStateModel): FolderOptions {
    return state.options;
  }

  @Selector([DocumentsState])
  public static getRootFolder(state: DocumentsStateModel): RDocuments | null {
    return state.rootFolder;
  }
  @Selector([DocumentsState])
  public static getPublicFolder(state: DocumentsStateModel): RDocuments | null {
    return state.publicFolder;
  }
  @Selector([DocumentsState])
  public static getFolderPath(state: DocumentsStateModel): FolderPath {
    return state.folderPath;
  }
  @Selector([DocumentsState])
  public static getFoldersTree(state: DocumentsStateModel): any {
    return state.foldersTree;
  }

  @Selector([DocumentsState])
  public static getFolders(state: DocumentsStateModel): TFolder[] | undefined {
    return state.rootFolder?.folders;
  }
  @Selector([DocumentsState])
  public static getDocumentsFromFolder(
    state: DocumentsStateModel
  ): TDocument[] | undefined {
    return state.rootFolder?.documents;
  }
  @Selector([DocumentsState])
  public static getSelectedDocumentFromFolder(state: DocumentsStateModel): any {
    return (documentPath: string) => {
      const documents = state.rootFolder?.documents;
      if (documents) {
        return documents.find((d) => d.path_url === documentPath);
      }
      return null;
    };
  }
  @Selector([DocumentsState])
  public static getCurrentFolder(
    state: DocumentsStateModel
  ): TFolder | null | undefined {
    return state.rootFolder?.currentFolder;
  }
  @Selector([DocumentsState])
  public static getSelectedDocument(
    state: DocumentsStateModel
  ): TDocument | null {
    return state.selectedDocument;
  }
  @Selector([DocumentsState])
  public static getPathAccessType(
    state: DocumentsStateModel
  ): DocumentAccessTypeEnum | null {
    return state.rootFolder?.accessType || null;
  }

  @Selector([DocumentsState])
  public static getSearchFolderHierarchy(
    state: DocumentsStateModel
  ): PaginationResponse<SearchFolderHierarchyModel> | null {
    return state.folderSearch;
  }
  @Selector([DocumentsState])
  public static getSearchDocuments(
    state: DocumentsStateModel
  ): PaginationResponse<SearchDocumentType> | null {
    return state.documentSearch;
  }
  @Selector([DocumentsState])
  public static searchInProgress(state: DocumentsStateModel): boolean {
    return state.searchInProgress;
  }

  @Selector([DocumentsState])
  public static getFolderPathPermissions(
    state: DocumentsStateModel
  ): UserPermissionType[] | null {
    return state.folderPathPermissions || null;
  }
  @Selector([DocumentsState])
  public static getUserPermissions(
    state: DocumentsStateModel
  ): UserPermissionsClass {
    return new UserPermissionsClass(state.rootFolder?.permissions);
  }
}
