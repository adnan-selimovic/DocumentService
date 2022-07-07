import { Component, OnInit } from '@angular/core';
import { Select, Store } from '@ngxs/store';
import {
  FolderPreviewType,
  RDocuments,
  FolderOptions,
  UserPermissionsClass,
} from '../../../../app-core/models';
import { Observable } from 'rxjs';
import { UploadDocument } from '../../store/documents.actions';
import { DocumentsSelectors } from '../../store/documents.selectors';

@Component({
  selector: 'app-documents-layout',
  templateUrl: './documents-layout.component.html',
  styleUrls: ['./documents-layout.component.scss'],
})
export class DocumentsLayoutComponent implements OnInit {
  @Select(DocumentsSelectors.getUserPermissions)
  getUserPermissions$!: Observable<UserPermissionsClass>;
  @Select(DocumentsSelectors.getRootFolder)
  getRootFolder$!: Observable<RDocuments>;
  @Select(DocumentsSelectors.getFolderOptions)
  getFolderOptions$!: Observable<FolderOptions>;

  getFolderOptions!: FolderOptions;
  folderPreviewType = FolderPreviewType;
  getUserPermissions!: UserPermissionsClass;

  constructor(private store: Store) {}

  ngOnInit(): void {
    this.getUserPermissions$.subscribe(
      (values) => (this.getUserPermissions = values)
    );
    this.getFolderOptions$.subscribe(
      (values) => (this.getFolderOptions = values)
    );
  }

  getDocumentsToUpload(files: File[]) {
    this.store.dispatch(new UploadDocument(null, files));
  }
}
