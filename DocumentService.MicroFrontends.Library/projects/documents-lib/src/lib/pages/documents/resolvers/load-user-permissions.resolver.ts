import { Injectable } from '@angular/core';
import {
  Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot,
} from '@angular/router';
import { Store } from '@ngxs/store';
import { map, Observable } from 'rxjs';
import { GetFolderPathPermissions } from '../store/documents.actions';
import { DocumentsSelectors } from '../store/documents.selectors';

@Injectable({
  providedIn: 'root',
})
export class LoadUserPermissionsResolver implements Resolve<boolean> {
  constructor(private store: Store) {}
  resolve(
    route: ActivatedRouteSnapshot,
    _state: RouterStateSnapshot
  ): Observable<any> {
    const qp = route.queryParams as any;
    const path = qp && qp.path ? qp.path : '/';
    return this.store.dispatch(new GetFolderPathPermissions(path)).pipe(
      map(() => {
        return this.store.selectSnapshot(
          DocumentsSelectors.getFolderPathPermissions
        );
      })
    );
  }
}
