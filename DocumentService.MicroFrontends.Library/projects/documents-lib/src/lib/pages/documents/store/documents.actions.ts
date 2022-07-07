import {
  QueryFilter,
  TDocument,
  FolderOptions,
  UserPermissionType,
  PathAccessTypeRequest,
} from '../../../app-core/models';

// service actions
export class LoadDocuments {
  static readonly type = '[Documents] LoadDocuments';
  constructor(public folderPath = '/', public forceLoad = false) {}
}
export class LoadPublicDocuments {
  static readonly type = '[Documents] LoadPublicDocuments';
  constructor(public folderPath: string) {}
}
export class LoadFolderHierarchy {
  static readonly type = '[Documents] LoadFolderHierarchy';
  constructor(public folderPath = '/') {}
}

// upload
export class UploadDocument {
  static readonly type = '[Documents] UploadDocument';
  constructor(public folderPath: string | null, public files: File[]) {}
}
export class UpdateDocument {
  static readonly type = '[Documents] UpdateDocument';
  constructor(public documentId: string, public file: File) {}
}
export class CreateFolder {
  static readonly type = '[Documents] CreateFolder';
  constructor(public folderName: string) {}
}

// search
export class SearchFoldersHierarchy {
  static readonly type = '[Documents] SearchFoldersHierarchy';
  constructor(public queryFilter: QueryFilter) {}
}
export class SearchThroughDocuments {
  static readonly type = '[Documents] SearchThroughDocuments';
  constructor(public queryFilter: QueryFilter) {}
}

// checkin - checkout
export class CheckInDocument {
  static readonly type = '[Documents] CheckInDocument';
  constructor(public documentId: string) {}
}
export class CheckOutDocument {
  static readonly type = '[Documents] CheckOutDocument';
  constructor(public documentId: string) {}
}

// delete - document - folder
export class DeleteDocument {
  static readonly type = '[Documents] DeleteDocument';
  constructor(public documentId: string) {}
}
export class DeleteFolder {
  static readonly type = '[Documents] DeleteFolder';
  constructor(public folderId: string) {}
}

// download
export class DownloadDocument {
  static readonly type = '[Documents] DownloadDocument';
  constructor(public documentId: string, public isPublic = false) {}
}
// get document text content
export class GetDocumentTextContent {
  static readonly type = '[Documents] GetDocumentTextContent';
  constructor(public documentId: string) {}
}

// folder path permissions
export class GetFolderPathPermissions {
  static readonly type = '[Documents] GetFolderPathPermissions';
  constructor(public folderPath = '/') {}
}
export class AddFolderPathPermissions {
  static readonly type = '[Documents] AddFolderPathPermissions';
  constructor(
    public folderPath = '/',
    public permissionObj: UserPermissionType
  ) {}
}
export class UpdateUserPermissionOnFolderPath {
  static readonly type = '[Documents] UpdateUserPermissionOnFolderPath';
  constructor(
    public folderPath = '/',
    public permissionObj: UserPermissionType
  ) {}
}
export class SetPathAccessType {
  static readonly type = '[Documents] SetPathAccessType';
  constructor(
    public folderPath = '/',
    public accessTypeObj: PathAccessTypeRequest
  ) {}
}
export class DeleteFolderPathPermissions {
  static readonly type = '[Documents] DeleteFolderPathPermissions';
  constructor(public folderPath = '/', public userId: string) {}
}

////////////////////--- H E L P E R S ---////////////////////
// helpers
export class SetFolderOptions {
  static readonly type = '[Documents] SetFolderOptions';
  constructor(public folderOptions: Partial<FolderOptions>) {}
}
export class SelectDocument {
  static readonly type = '[Documents] SelectDocument';
  constructor(public document: TDocument | null) {}
}
