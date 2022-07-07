// import { Injectable } from '@angular/core';
// import {
//   Router, Resolve,
//   RouterStateSnapshot,
//   ActivatedRouteSnapshot
// } from '@angular/router';
// import { Observable, of } from 'rxjs';

// @Injectable({
//   providedIn: 'root'
// })
// export class LoadPublicDocumentsResolver implements Resolve<boolean> {
//   resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
//     return of(true);
//   }
// }

import { Injectable } from '@angular/core';
import {
  Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot,
  Router,
} from '@angular/router';
import { Store } from '@ngxs/store';
import { map, of } from 'rxjs';
import { Observable } from 'rxjs';
import { LoadDocuments, LoadPublicDocuments } from '../store/documents.actions';
import { DocumentsState } from '../store/documents.state';

@Injectable({
  providedIn: 'any',
})
export class LoadPublicDocumentsResolver implements Resolve<any> {
  constructor(private store: Store, private router: Router) {}
  resolve(
    route: ActivatedRouteSnapshot,
    _state: RouterStateSnapshot
  ): Observable<any> {
    const qp = route.queryParams as any;
    const path = qp && qp.path ? qp.path : null;

    if (path === null) {
      this.router.navigateByUrl('/unauthorized');
    }

    return this.store.dispatch(new LoadPublicDocuments(path)).pipe(
      map(() => {
        return this.store.selectSnapshot(DocumentsState);
      })
    );
  }
}
