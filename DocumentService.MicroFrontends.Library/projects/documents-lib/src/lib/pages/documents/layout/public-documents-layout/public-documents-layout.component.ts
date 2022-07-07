import { Component, OnInit } from '@angular/core';
import { Select, Store } from '@ngxs/store';
import { DocumentHelperClass, RDocuments } from '../../../../app-core/models';
import { Observable } from 'rxjs';
import { DownloadDocument } from '../../store/documents.actions';
import { DocumentsSelectors } from '../../store/documents.selectors';

@Component({
  selector: 'lib-public-documents-layout',
  templateUrl: './public-documents-layout.component.html',
  styleUrls: ['./public-documents-layout.component.css'],
})
export class PublicDocumentsLayoutComponent implements OnInit {
  @Select(DocumentsSelectors.getPublicFolder)
  getPublicFolder$!: Observable<RDocuments>;

  constructor(private store: Store) {}

  ngOnInit(): void {}

  getDocumentSize(size: number): string {
    return DocumentHelperClass.getSize(size).textValue;
  }
  download(documentId: string): void {
    this.store.dispatch(new DownloadDocument(documentId));
  }
}
