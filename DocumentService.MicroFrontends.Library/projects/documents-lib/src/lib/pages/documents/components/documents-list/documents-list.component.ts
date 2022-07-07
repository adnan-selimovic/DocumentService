import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Select } from '@ngxs/store';
import { TDocument, TFolder } from '../../../../app-core/models';
import { Observable } from 'rxjs';
import { DocumentsSelectors } from '../../store/documents.selectors';

@Component({
  selector: 'app-documents-list',
  templateUrl: './documents-list.component.html',
  styleUrls: ['./documents-list.component.scss'],
})
export class DocumentsListComponent implements OnInit {
  @Select(DocumentsSelectors.getFolders) folders$!: Observable<TFolder[]>;
  @Select(DocumentsSelectors.getDocumentsFromFolder) documents$!: Observable<
    TDocument[]
  >;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {}
}
