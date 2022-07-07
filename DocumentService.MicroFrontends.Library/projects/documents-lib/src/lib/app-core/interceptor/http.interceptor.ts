import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpHeaders,
  HttpErrorResponse,
} from '@angular/common/http';
import { catchError, map, Observable, throwError } from 'rxjs';
import { StorageService } from '../utils/storage.service';
import { StorageEnum } from '../models';
import { Store } from '@ngxs/store';
import {
  Authorize,
  Unauthorized,
} from '../../shared/stores/auth-store/auth.actions';

@Injectable()
export class HttpRequestInterceptor implements HttpInterceptor {
  constructor(private _storageService: StorageService, private store: Store) {}

  getHeaders(url: string = '') {
    const token = this._storageService.getItem<string>(StorageEnum.TOKEN) || '';
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });
  }

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    if (request.url.includes('/connect/token')) return next.handle(request);

    request = this.addTokenHeader(request);
    return next.handle(request).pipe(
      catchError((error) => {
        if (
          error instanceof HttpErrorResponse &&
          !request.url.includes('/connect/token') &&
          error.status === 401
        ) {
          return this.handle401Error(request, next);
        }

        return throwError(error);
      })
    );
  }

  private addTokenHeader(request: HttpRequest<any>) {
    const headers = this.getHeaders();
    return request.clone({ headers });
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler) {
    return this.store.dispatch(new Authorize()).pipe(
      map((res) => {
        return next.handle(this.addTokenHeader(request));
      }),
      catchError((err) => {
        return this.store.dispatch(new Unauthorized());
      })
    );
  }
}
