import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngxs/store';
import { LoadDocuments } from '../../store/documents.actions';

@Component({
  selector: 'app-main-document-layout',
  templateUrl: './main-document-layout.component.html',
  styleUrls: ['./main-document-layout.component.scss'],
})
export class MainDocumentLayoutComponent implements OnInit {
  constructor(private store: Store, private route: ActivatedRoute) {
    this.route.queryParams.subscribe((qp: any) => {
      qp.path && this.loadDocuments(qp.path);
    });
  }

  ngOnInit(): void {}

  loadDocuments(folderPath: string) {
    this.store.dispatch(new LoadDocuments(folderPath));
  }
}
