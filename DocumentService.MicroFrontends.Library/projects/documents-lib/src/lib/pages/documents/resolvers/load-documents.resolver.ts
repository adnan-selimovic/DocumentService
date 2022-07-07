import { Injectable } from '@angular/core';
import {
  Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot,
} from '@angular/router';
import { Store } from '@ngxs/store';
import { map } from 'rxjs';
import { Observable } from 'rxjs';
import { LoadDocuments } from '../store/documents.actions';
import { DocumentsState } from '../store/documents.state';

@Injectable({
  providedIn: 'any',
})
export class LoadDocumentsResolver implements Resolve<any> {
  constructor(private store: Store) {}
  resolve(
    route: ActivatedRouteSnapshot,
    _state: RouterStateSnapshot
  ): Observable<any> {
    const qp = route.queryParams as any;
    const path = qp && qp.path ? qp.path : '/';
    return this.store.dispatch(new LoadDocuments(path)).pipe(
      map(() => {
        return this.store.selectSnapshot(DocumentsState);
      })
    );
  }
}
