import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { DocumentsLibModule } from 'documents-lib';
import { NgxsModule } from '@ngxs/store';
import { environment } from 'src/environments/environment';
import { NgxsReduxDevtoolsPluginModule } from '@ngxs/devtools-plugin';

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,

    NgxsModule.forRoot([], {
      developmentMode: !environment.production,
    }),
    NgxsReduxDevtoolsPluginModule.forRoot({
      disabled: environment.production,
    }),

    DocumentsLibModule.forRoot({
      apiConfiguration: {
        webAppUrl: 'http://localhost:4200',
        documentServiceUrl:
          'https://documentservice-polyglotpersistence.azurewebsites.net',
        authServiceUrl:
          'https://identityserver-documentservice.azurewebsites.net',
        authConfig: [
          { key: 'client_id', value: 'qssClient' },
          { key: 'client_secret', value: 'QssDocs123!' },
          { key: 'scope', value: 'documentservice.read' },
          { key: 'grant_type', value: 'password' },
          { key: 'username', value: 'qssbh' },
          { key: 'password', value: 'qssbh' },
        ],
      },
    }),
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
