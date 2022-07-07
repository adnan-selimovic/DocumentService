import { ModuleWithProviders, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { DocumentsLibRoutingModule } from './documents-lib-routing.module';
import { DocumentsLibComponent } from './documents-lib.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { GlobalComponentsModule } from './shared/global-components/global-components.module';
import { NgxsModule } from '@ngxs/store';
import { environment } from '../environments/environment';
import { NgxsReduxDevtoolsPluginModule } from '@ngxs/devtools-plugin';
import { AuthState } from './shared/stores/auth-store/auth.state';
import { DocumentsState } from './pages/documents/store/documents.state';
import { AppCoreModule } from '../public-api';
import { ReactiveFormsModule } from '@angular/forms';
import { MatRippleModule } from '@angular/material/core';
import { NgxDocViewerModule } from 'ngx-doc-viewer';
import { DocumentsLibService } from './documents-lib.service';
import { ModuleConfigModel } from './app-core/configs/module-config/module-config.model';
import { CodeEditorModule } from '@ngstack/code-editor';

@NgModule({
  declarations: [DocumentsLibComponent],
  imports: [
    BrowserModule,
    DocumentsLibRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    GlobalComponentsModule,

    AppCoreModule,
    ReactiveFormsModule,
    MatRippleModule,
    NgxDocViewerModule,

    NgxsModule.forFeature([AuthState, DocumentsState]),
    NgxsReduxDevtoolsPluginModule.forRoot({
      disabled: environment.production,
    }),

    CodeEditorModule.forRoot(),
  ],
  exports: [DocumentsLibComponent],
  providers: [],
  bootstrap: [DocumentsLibComponent],
})
export class DocumentsLibModule {
  static forRoot(
    configuration: ModuleConfigModel
  ): ModuleWithProviders<DocumentsLibModule> {
    return {
      ngModule: DocumentsLibModule,
      providers: [
        DocumentsLibService,
        { provide: 'moduleConfiguration', useValue: configuration },
      ],
    };
  }
}
