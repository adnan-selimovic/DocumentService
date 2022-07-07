import { Injectable } from '@angular/core';
import {
  Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot,
} from '@angular/router';
import { Store } from '@ngxs/store';
import { map, Observable } from 'rxjs';
import { RDocuments } from '../../../app-core/models';
import { SelectDocument } from '../store/documents.actions';
import { DocumentsSelectors } from '../store/documents.selectors';

@Injectable({
  providedIn: 'root',
})
export class SelectedDocumentResolver implements Resolve<any> {
  constructor(private store: Store) {}
  resolve(
    route: ActivatedRouteSnapshot,
    _state: RouterStateSnapshot
  ): Observable<any> {
    const qp = route.queryParams as any;
    const path = qp && qp.path ? qp.path : '/';
    const rootFolder = this.store.selectSnapshot(
      (state) => state.documents.rootFolder
    ) as RDocuments;
    const document = rootFolder.documents.find((d) => d.path_url === path);

    return this.store.dispatch(new SelectDocument(document || null)).pipe(
      map(() => {
        return this.store.selectSnapshot(
          DocumentsSelectors.getSelectedDocument
        );
      })
    );
  }
}
