import { Component, OnInit } from '@angular/core';
import { Select } from '@ngxs/store';
import { TDocument, TFolder } from '../../../../app-core/models';
import { Observable } from 'rxjs';
import { DocumentsSelectors } from '../../store/documents.selectors';

@Component({
  selector: 'app-documents-icon-list',
  templateUrl: './documents-icon-list.component.html',
  styleUrls: ['./documents-icon-list.component.scss'],
})
export class DocumentsIconListComponent implements OnInit {
  @Select(DocumentsSelectors.getFolders) folders$!: Observable<TFolder[]>;
  @Select(DocumentsSelectors.getDocumentsFromFolder) documents$!: Observable<
    TDocument[]
  >;

  constructor() {}

  ngOnInit(): void {}
}
