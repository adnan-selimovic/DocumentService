import { Injectable } from '@angular/core';
import {
  Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot,
  Router,
} from '@angular/router';
import { StorageEnum, AuthResponseModel } from '../models';
import { StorageService } from '../utils/storage.service';
import { Store } from '@ngxs/store';
import { map, Observable, of } from 'rxjs';
import { Authorize } from '../../shared/stores/auth-store/auth.actions';
import { AuthSelectors } from '../../shared/stores/auth-store/auth.selectors';
import { DateHelpers } from '../helpers';

@Injectable({
  providedIn: 'root',
})
export class AuthResolver implements Resolve<boolean> {
  constructor(
    private store: Store,
    private _storageService: StorageService,
    private router: Router
  ) {}

  resolve(
    _route: ActivatedRouteSnapshot,
    _state: RouterStateSnapshot
  ): Observable<any> {
    const authObj = this._storageService.getItem<AuthResponseModel>(
      StorageEnum.AUTHORIZATION
    );

    if (this.router.url.includes('/documetns/private')) {
      return of(true);
    }

    if (
      !authObj ||
      (authObj?.expires_on &&
        DateHelpers.datesDiffInMinutes(authObj?.expires_on, new Date()) < 1)
    ) {
      return this.store.dispatch(new Authorize()).pipe(
        map(() => {
          return this.store.selectSnapshot(AuthSelectors.getAuth);
        })
      );
    }

    return of(true);
  }
}
