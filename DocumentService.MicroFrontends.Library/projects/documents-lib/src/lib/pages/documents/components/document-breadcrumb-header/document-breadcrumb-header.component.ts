import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import {
  FolderPath,
  FolderPreviewType,
  DialogData,
  TDocument,
  FolderOptions,
  TFolder,
  UserPermissionsClass,
  PathAccessTypeRequest,
} from '../../../../app-core/models';
import { Select, Store } from '@ngxs/store';
import {
  CheckInDocument,
  CheckOutDocument,
  DeleteDocument,
  DeleteFolder,
  DownloadDocument,
  SetFolderOptions,
  SetPathAccessType,
} from '../../../documents/store/documents.actions';
import { DocumentsSelectors } from '../../../documents/store/documents.selectors';
import { Observable } from 'rxjs';
import { CreateNewFolderModalComponent } from '../create-new-folder-modal/create-new-folder-modal.component';
import { ConfirmActionMessageComponent } from '../../../../shared/global-components/messages/confirm-action-message/confirm-action-message.component';
import { DocumentAccessTypeEnum } from '../../../..//app-core/models/enums/document-access-type.enum';
import { UserPermissionsEnum } from '../../../../app-core/models/enums/user-permissions.enum';
import { DocumentsLibService } from '../../../../documents-lib.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-document-breadcrumb-header',
  templateUrl: './document-breadcrumb-header.component.html',
  styleUrls: ['./document-breadcrumb-header.component.scss'],
})
export class DocumentBreadcrumbHeaderComponent implements OnInit {
  @Select(DocumentsSelectors.getFolderPath)
  getFolderPath$!: Observable<FolderPath>;
  @Select(DocumentsSelectors.getFolderOptions)
  getFolderOptions$!: Observable<FolderOptions>;
  @Select(DocumentsSelectors.getCurrentFolder)
  getCurrentFolder$!: Observable<TFolder>;
  @Select(DocumentsSelectors.getSelectedDocument)
  getSelectedDocument$!: Observable<TDocument>;
  @Select(DocumentsSelectors.getUserPermissions)
  getUserPermissions$!: Observable<UserPermissionsClass>;
  @Select(DocumentsSelectors.getPathAccessType)
  getPathAccessType$!: Observable<DocumentAccessTypeEnum>;

  documentAccessTypeEnum = DocumentAccessTypeEnum;

  getFolderPath!: FolderPath;
  currentFolder?: TFolder | null;
  selectedDocument!: TDocument | null;
  folderPreviewType = FolderPreviewType;
  getPathAccessType!: DocumentAccessTypeEnum;
  isDocument = false;

  form = this.fb.group({
    searchFolderPath: [''],
  });
  get searchFolderPath() {
    return this.form.get('searchFolderPath');
  }

  constructor(
    private store: Store,
    private fb: FormBuilder,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
    private _documentsLibService: DocumentsLibService,
    private snackBar: MatSnackBar
  ) {
    this.getFolderPath$.subscribe((values) => {
      this.getFolderPath = values;
      this.form.patchValue({ searchFolderPath: values.folderPath });
    });
    this.checkForDocument();
  }

  ngOnInit(): void {
    this.getCurrentFolder$.subscribe((values) => (this.currentFolder = values));
    this.getSelectedDocument$.subscribe(
      (values) => (this.selectedDocument = values)
    );
    this.getPathAccessType$.subscribe(
      (values) => (this.getPathAccessType = values)
    );
  }

  setPreviewType(type: FolderPreviewType) {
    this.store.dispatch(new SetFolderOptions({ previewType: type }));
  }

  addNewFolder(): void {
    let dialogData: DialogData;
    dialogData = {
      title: 'Create New Folder',
      showConfirm: true,
    };
    this.dialog.open(CreateNewFolderModalComponent, {
      width: '50%',
      data: dialogData,
    });
  }

  checkIn(): void {
    if (this.selectedDocument) {
      const documentId = this.selectedDocument._id;
      let dialogData: DialogData;
      dialogData = {
        title: 'Confirm Your Action!',
        description: `Are you sure you want to check-in the document ${this.selectedDocument.document_name}?`,
        showConfirm: true,
        onConfirm: () =>
          this.store
            .dispatch(new CheckInDocument(documentId))
            .subscribe(() => this.dialog.closeAll()),
      };
      this.dialog.open(ConfirmActionMessageComponent, {
        width: '50%',
        data: dialogData,
      });
    }
  }
  checkOut(): void {
    if (this.selectedDocument) {
      const documentId = this.selectedDocument._id;
      let dialogData: DialogData;
      dialogData = {
        title: 'Confirm Your Action!',
        description: `Are you sure you want to check-out the document ${this.selectedDocument.document_name}?`,
        showConfirm: true,
        onConfirm: () =>
          this.store
            .dispatch(new CheckOutDocument(documentId))
            .subscribe(() => this.dialog.closeAll()),
      };
      this.dialog.open(ConfirmActionMessageComponent, {
        width: '50%',
        data: dialogData,
      });
    }
  }
  setPathAccessType(): void {
    const accessType =
      this.getPathAccessType === this.documentAccessTypeEnum.PRIVATE
        ? this.documentAccessTypeEnum.PUBLIC
        : this.documentAccessTypeEnum.PRIVATE;
    const accessTypeObj: PathAccessTypeRequest = {
      accessType,
      permissions: [UserPermissionsEnum.READ, UserPermissionsEnum.WRITE],
    };
    let dialogData: DialogData;
    dialogData = {
      title: `${
        accessType === DocumentAccessTypeEnum.PRIVATE
          ? 'Set Private Action'
          : 'Set Public Action'
      }`,
      description: `Are you sure you want to set ${
        accessType === DocumentAccessTypeEnum.PRIVATE ? 'private' : 'public'
      } access?${
        accessType === DocumentAccessTypeEnum.PUBLIC
          ? '\nPublic users will have read and write rights.'
          : '\nOnly logged in users with certain permissions will be able to read the document.'
      }`,
      showConfirm: true,
      onConfirm: () =>
        this.store
          .dispatch(
            new SetPathAccessType(this.getFolderPath.folderPath, accessTypeObj)
          )
          .subscribe(() => {
            this.dialog.closeAll();
          }),
    };
    this.dialog.open(ConfirmActionMessageComponent, {
      width: '50%',
      data: dialogData,
    });
  }
  deleteDocument(): void {
    if (this.selectedDocument) {
      const documentId = this.selectedDocument._id;
      let dialogData: DialogData;
      dialogData = {
        title: 'Confirm Your Action!',
        description: `Are you sure you want to delete the document ${this.selectedDocument.document_name}?`,
        showConfirm: true,
        onConfirm: () =>
          this.store.dispatch(new DeleteDocument(documentId)).subscribe(() => {
            this.dialog.closeAll();
            const previousFolderPath = this.getFolderPath.getPreviousFolder();
            previousFolderPath &&
              this.router.navigate(['/documents'], {
                queryParams: { path: previousFolderPath },
              });
          }),
      };
      this.dialog.open(ConfirmActionMessageComponent, {
        width: '50%',
        data: dialogData,
      });
    }
  }
  deleteFolder(): void {
    if (this.currentFolder) {
      const folderId = this.currentFolder._id;
      let dialogData: DialogData;
      dialogData = {
        title: 'Confirm Your Action!',
        description: `Are you sure you want to delete the folder ${this.currentFolder.folder_name}? \nDeleteing a folder deletes all documents.`,
        showConfirm: true,
        onConfirm: () =>
          this.store.dispatch(new DeleteFolder(folderId)).subscribe(() => {
            this.dialog.closeAll();
            const previousFolderPath = this.getFolderPath.getPreviousFolder();
            previousFolderPath &&
              this.router.navigate(['/documents'], {
                queryParams: { path: previousFolderPath },
              });
          }),
      };
      this.dialog.open(ConfirmActionMessageComponent, {
        width: '50%',
        data: dialogData,
      });
    }
  }

  downloadDocument(): void {
    this.selectedDocument &&
      this.store.dispatch(new DownloadDocument(this.selectedDocument._id));
  }

  // helpers
  getShareLink(): string {
    return `${this._documentsLibService.webAppUrl}/documents/public?path=${this.getFolderPath.folderPath}`;
  }
  copyLink(): void {
    this.snackBar.open('Link Copied!', undefined, { duration: 3000 });
  }

  // listeners
  checkForDocument() {
    this.route.queryParams.subscribe((queryParams: any) => {
      if (queryParams.document) {
        this.isDocument = true;
      } else {
        this.isDocument = false;
      }
    });
  }
}
