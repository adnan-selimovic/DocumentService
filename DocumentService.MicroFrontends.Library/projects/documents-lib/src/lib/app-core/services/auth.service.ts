import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { DocumentsLibService } from '../../documents-lib.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  authServiceUrl!: string;
  constructor(
    private http: HttpClient,
    private _documentsLibService: DocumentsLibService
  ) {
    this.authServiceUrl = this._documentsLibService.authServiceUrl;
  }

  authorize() {
    const body = new URLSearchParams();

    // set auth configuration
    this._documentsLibService.apiConfiguration.authConfig &&
      this._documentsLibService.apiConfiguration.authConfig.forEach(
        (element) => {
          body.set(element.key, element.value);
        }
      );

    const httpLoginOptions = this.getUrlEncodedHeaders();
    return this.http.post(
      `${this.authServiceUrl}/connect/token`,
      body.toString(),
      httpLoginOptions
    );
  }

  refreshToken() {
    const body = new URLSearchParams();
    body.set('refresh_token', '');
    body.set('client_id', '');
    body.set('client_secret', '');
    body.set('grant_type', 'refresh_token');

    const httpLoginOptions = this.getUrlEncodedHeaders();
    return this.http.post(
      `${this.authServiceUrl}/connect/token`,
      body.toString(),
      httpLoginOptions
    );
  }

  private getUrlEncodedHeaders() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/x-www-form-urlencoded',
      }),
    };
  }
}
