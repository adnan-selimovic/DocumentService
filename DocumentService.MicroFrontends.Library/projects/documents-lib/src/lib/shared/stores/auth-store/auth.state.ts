import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Action, NgxsOnInit, State, StateContext } from '@ngxs/store';
import { catchError, finalize, tap, throwError } from 'rxjs';
import { StorageEnum, AuthResponseModel } from '../../../app-core/models';
import { AuthService } from '../../../app-core/services/auth.service';
import { StorageService } from '../../../app-core/utils/storage.service';
import { AuthStateModel, defaultState } from './auth-state.model';
import { Authorize, Unauthorized } from './auth.actions';

@State<AuthStateModel>({
  name: 'auth',
  defaults: defaultState,
})
@Injectable()
export class AuthState implements NgxsOnInit {
  constructor(
    private _authService: AuthService,
    private _storageService: StorageService,
    private router: Router
  ) {}

  ngxsOnInit(ctx?: StateContext<AuthStateModel>) {
    const authObj = this._storageService.getItem<AuthResponseModel>(
      StorageEnum.AUTHORIZATION
    );
    if (authObj) {
      ctx?.patchState({
        auth: authObj,
      });
    }
  }

  // A U T H O R I Z E
  @Action(Authorize)
  Authorize(ctx: StateContext<AuthStateModel>): any {
    ctx.patchState({
      loading: true,
      error: null,
    });
    return this._authService.authorize().pipe(
      tap((response: any) => {
        const responseData = response as AuthResponseModel;
        let expiresOn = new Date();
        expiresOn.setSeconds(expiresOn.getSeconds() + responseData.expires_in);
        responseData.expires_on = expiresOn;

        this._storageService.setItem(
          StorageEnum.TOKEN,
          JSON.stringify(responseData.access_token)
        );
        this._storageService.setItem(
          StorageEnum.AUTHORIZATION,
          JSON.stringify(responseData)
        );

        ctx.patchState({
          loading: false,
          auth: responseData,
        });
      }),
      catchError((err) => {
        ctx.dispatch(new Unauthorized());
        ctx.patchState({ error: err });
        return throwError(err);
      }),
      finalize(() => {
        ctx.patchState({ loading: false });
      })
    );
  }

  @Action(Unauthorized)
  Unauthorized(ctx: StateContext<AuthStateModel>): any {
    this.router.navigateByUrl('/unauthorized');
  }
}
