import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Select, Store } from '@ngxs/store';
import { environment } from '../../../../../environments/environment';
import {
  QueryFilter,
  PaginationResponse,
  SearchDocumentType,
} from '../../../../app-core/models';
import { Observable } from 'rxjs';
import { SearchThroughDocuments } from '../../store/documents.actions';
import { DocumentsSelectors } from '../../store/documents.selectors';

@Component({
  selector: 'app-search-documents-layout',
  templateUrl: './search-documents-layout.component.html',
  styleUrls: ['./search-documents-layout.component.scss'],
})
export class SearchDocumentsLayoutComponent implements OnInit {
  @Select(DocumentsSelectors.searchInProgress)
  searchInProgress$!: Observable<boolean>;
  searchInProgress = false;
  @Select(DocumentsSelectors.getSearchDocuments)
  getSearchDocuments$!: Observable<PaginationResponse<SearchDocumentType>>;
  getSearchDocuments!: PaginationResponse<SearchDocumentType>;

  queryFilter = new QueryFilter();
  url!: string;
  show = false;
  selectedFileId?: string;
  isContentLoaded = false;

  constructor(private store: Store, private route: ActivatedRoute) {
    this.route.queryParams.subscribe((qp: any) => {
      if (qp && qp.text) {
        this.queryFilter.searchTerm = qp.text;
        this.searchDocuments();
      }
    });
  }

  ngOnInit(): void {
    this.searchInProgress$.subscribe(
      (value) => (this.searchInProgress = value)
    );
    this.getSearchDocuments$.subscribe((values) => {
      if (values) {
        this.getSearchDocuments = values;
        if (values.totalSize > 0) {
          this.showDocument(values.items[0].id);
        }
      }
    });
  }

  searchDocuments() {
    this.store.dispatch(new SearchThroughDocuments(this.queryFilter));
  }

  showDocument(fileId: string) {
    this.isContentLoaded = false;
    this.selectedFileId = fileId;
    this.url = `${environment.documentServiceUrl}/api/Document/${fileId}`;
    setTimeout(() => {
      this.show = true;
    }, 1000);
  }

  contentLoaded() {
    this.isContentLoaded = true;
  }
}
