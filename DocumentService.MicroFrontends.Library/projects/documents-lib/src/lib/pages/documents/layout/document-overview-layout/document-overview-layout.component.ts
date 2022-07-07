import { Component, OnDestroy, OnInit } from '@angular/core';
import { Select, Store } from '@ngxs/store';
import {
  DownloadDocument,
  SelectDocument,
  UpdateDocument,
} from '../../store/documents.actions';
import {
  DocumentHelperClass,
  TDocument,
  UserPermissionsClass,
} from '../../../../app-core/models';
import { FormBuilder } from '@angular/forms';
import { DocumentsSelectors } from '../../store/documents.selectors';
import { Observable } from 'rxjs';
import { DocumentAccessTypeEnum } from '../../../../app-core/models/enums/document-access-type.enum';
import { DocumentsLibService } from '../../../../documents-lib.service';
import { CodeModel } from '@ngstack/code-editor';

@Component({
  selector: 'app-document-overview-layout',
  templateUrl: './document-overview-layout.component.html',
  styleUrls: ['./document-overview-layout.component.scss'],
})
export class DocumentOverviewLayoutComponent implements OnInit, OnDestroy {
  @Select(DocumentsSelectors.getSelectedDocument)
  getSelectedDocument$!: Observable<TDocument>;
  @Select(DocumentsSelectors.getUserPermissions)
  getUserPermissions$!: Observable<UserPermissionsClass>;
  @Select(DocumentsSelectors.getPathAccessType)
  getPathAccessType$!: Observable<DocumentAccessTypeEnum>;

  documentAccessTypeEnum = DocumentAccessTypeEnum;
  getPathAccessType!: DocumentAccessTypeEnum;
  getUserPermissions!: UserPermissionsClass;
  textDocumentType: 'TEXT_EDITOR' | 'CODE_EDITOR' = 'TEXT_EDITOR';

  SELECTED_DOCUMENT!: TDocument;
  isContentLoaded = false;
  isTextContent = false; // check if folder or document
  documentUrl!: string;

  isEditable = false;

  form = this.fb.group({
    editor: [''],
  });
  get editor() {
    return this.form.get('editor');
  }

  editorStyles = {
    height: '93%',
  };

  // code editor
  codeModel!: CodeModel;
  CodeEditorOptions = {
    lineNumbers: true,
    contextmenu: false,
    minimap: {
      enabled: false,
    },
  };
  onCodeChanged(value: any) {
    this.editor?.setValue(value);
  }
  // code editor

  constructor(
    private store: Store,
    private fb: FormBuilder,
    private _documentsLibService: DocumentsLibService
  ) {}

  ngOnInit(): void {
    this.getPathAccessType$.subscribe((values) => {
      this.getPathAccessType = values;
    });

    this.getUserPermissions$.subscribe(
      (values) => (this.getUserPermissions = values)
    );

    this.getSelectedDocument$.subscribe((document) => {
      this.documentUrl = `${this._documentsLibService.documentServiceUrl}/api/Document/${document._id}/public`;
      this.isEditable = document.checked_out_by ? true : false;
      // check document content
      if (DocumentHelperClass.isTextFile(document.content_type)) {
        this.isTextContent = true;
      } else {
        this.isTextContent = false;
      }

      if (document.text_content) {
        this.isContentLoaded = true;
        this.editor?.setValue(document.text_content);
        this.codeModel = {
          language: 'text',
          uri: this.SELECTED_DOCUMENT.document_name,
          value: document.text_content,
        };
      }

      this.SELECTED_DOCUMENT = document;
    });

    // quill editor updates code editor
    this.editor?.valueChanges.subscribe((value) => {
      this.codeModel = {
        ...this.codeModel,
        value,
      };
    });
  }

  ngOnDestroy(): void {
    this.store.dispatch(new SelectDocument(null));
  }

  contentLoaded(): void {
    this.isContentLoaded = true;
  }

  getDocumentsToUpload(files: File[]): void {
    if (files.length === 1) {
      this.store.dispatch(
        new UpdateDocument(this.SELECTED_DOCUMENT._id, files[0])
      );
    }
  }

  download(): void {
    this.SELECTED_DOCUMENT &&
      this.store.dispatch(
        new DownloadDocument(
          this.SELECTED_DOCUMENT._id,
          this.getPathAccessType === this.documentAccessTypeEnum.PUBLIC
        )
      );
  }

  updateTextDocument() {
    const myBlob = new Blob([this.editor?.value], { type: 'text/plain' });
    const file = new File([myBlob], this.SELECTED_DOCUMENT.document_name, {
      lastModified: new Date().getTime(),
      type: myBlob.type,
    });

    this.store.dispatch(new UpdateDocument(this.SELECTED_DOCUMENT._id, file));
  }
}
