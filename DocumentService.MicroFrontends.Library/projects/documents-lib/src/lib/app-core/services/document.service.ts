import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  PathAccessTypeRequest,
  QueryFilter,
  UserPermissionType,
} from '../models';
import { Observable } from 'rxjs';
import { DocumentsLibService } from '../../documents-lib.service';
import { FileSaverService } from 'ngx-filesaver';

@Injectable({
  providedIn: 'root',
})
export class DocumentService {
  documentServiceUrl!: string;
  constructor(
    private http: HttpClient,
    private _documentsLibService: DocumentsLibService
  ) {
    this.documentServiceUrl = this._documentsLibService.documentServiceUrl;
  }

  getDocuments(folderPath = '/'): Observable<any> {
    return this.http.get(
      `${this.documentServiceUrl}/api/Folder?folderPath=${folderPath}`
    );
  }

  getPublicDocuments(folderPath: string): Observable<any> {
    return this.http.get(
      `${this.documentServiceUrl}/api/Folder/public?folderPath=${folderPath}`
    );
  }

  getFolderHierarchy(folderPath = '/'): Observable<any> {
    return this.http.get(
      `${this.documentServiceUrl}/api/Document/hierarchy?folderPath=${folderPath}`
    );
  }

  uploadDocument(folderPath: string, files: File[]): Observable<any> {
    const formData = new FormData();
    formData.append('folderPath', folderPath);
    files.forEach((file) => formData.append('postedDocuments', file));
    return this.http.post(`${this.documentServiceUrl}/api/Document`, formData, {
      reportProgress: true,
    });
  }
  updateDocument(documentId: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('documentId', documentId);
    formData.append('postedDocuments', file);
    return this.http.put(`${this.documentServiceUrl}/api/Document`, formData, {
      reportProgress: true,
    });
  }

  getDocumentTextContent(documentId: string): Observable<any> {
    return this.http.get(
      `${this.documentServiceUrl}/api/Document/${documentId}/text`
    );
  }
  downloadDocument(documentId: string, isPublic = false): Observable<any> {
    return this.http.get(
      isPublic
        ? `${this.documentServiceUrl}/api/Document/${documentId}/public`
        : `${this.documentServiceUrl}/api/Document/${documentId}`,
      {
        responseType: 'blob',
      }
    );
  }

  createFolder(folderPath = '/', folderName: string): Observable<any> {
    return this.http.post(
      `${this.documentServiceUrl}/api/Folder?folderPath=${folderPath}&folderName=${folderName}`,
      {}
    );
  }

  searchFoldersHierarchy(queryFilter: QueryFilter): Observable<any> {
    const queryString = queryFilter.getQueryString();
    return this.http.get(
      `${this.documentServiceUrl}/api/Folder/SearchByPath?${queryString}`
    );
  }
  searchThroughDocuments(queryFilter: QueryFilter): Observable<any> {
    const queryString = queryFilter.getQueryString();
    return this.http.get(
      `${this.documentServiceUrl}/api/Document/SearchByContent?${queryString}`
    );
  }

  checkInDocument(documentId: string): Observable<any> {
    return this.http.post(
      `${this.documentServiceUrl}/api/Document/${documentId}/checkin`,
      {}
    );
  }
  checkOutDocument(documentId: string): Observable<any> {
    return this.http.post(
      `${this.documentServiceUrl}/api/Document/${documentId}/checkout`,
      {}
    );
  }

  deleteFolder(folderId: string): Observable<any> {
    return this.http.delete(
      `${this.documentServiceUrl}/api/Folder?folderId=${folderId}`
    );
  }
  deleteDocument(documentId: string): Observable<any> {
    return this.http.delete(
      `${this.documentServiceUrl}/api/Document?documentId=${documentId}`
    );
  }

  // folder path permissions
  getFolderPathPermissions(
    folderPath = '/',
    folderId: string = '', // root ==== folder id || file id
    parentFolderId: string = '' // destination === parent folder id || folder id
  ): Observable<any> {
    return this.http.get(
      `${this.documentServiceUrl}/api/Permission?folderPath=${folderPath}&folderId=${folderId}&parentFolderId=${parentFolderId}`
    );
  }
  addFolderPathPermissions(
    folderPath = '/',
    folderId: string = '', // root ==== folder id || file id
    parentFolderId: string = '', // destination === parent folder id || folder id
    permissionObj: UserPermissionType
  ): Observable<any> {
    return this.http.post(
      `${this.documentServiceUrl}/api/Permission?folderPath=${folderPath}&folderId=${folderId}&parentFolderId=${parentFolderId}`,
      permissionObj
    );
  }
  updateUserPermissionOnFolderPath(
    folderPath = '/',
    folderId: string = '', // root ==== folder id || file id
    parentFolderId: string = '', // destination === parent folder id || folder id
    permissionObj: UserPermissionType
  ): Observable<any> {
    return this.http.patch(
      `${this.documentServiceUrl}/api/Permission?folderPath=${folderPath}&folderId=${folderId}&parentFolderId=${parentFolderId}`,
      permissionObj
    );
  }
  setPathAccessType(
    folderPath = '/',
    folderId: string = '', // root ==== folder id || file id
    parentFolderId: string = '', // destination === parent folder id || folder id
    accessTypeObj: PathAccessTypeRequest
  ): Observable<any> {
    return this.http.post(
      `${this.documentServiceUrl}/api/Permission/accessType?folderPath=${folderPath}&folderId=${folderId}&parentFolderId=${parentFolderId}`,
      accessTypeObj
    );
  }
  deleteFolderPathPermission(
    folderPath = '/',
    folderId: string = '', // root ==== folder id || file id
    parentFolderId: string = '', // destination === parent folder id || folder id
    userId: string
  ): Observable<any> {
    return this.http.delete(
      `${this.documentServiceUrl}/api/Permission?folderPath=${folderPath}&folderId=${folderId}&parentFolderId=${parentFolderId}&userId=${userId}`
    );
  }
}
