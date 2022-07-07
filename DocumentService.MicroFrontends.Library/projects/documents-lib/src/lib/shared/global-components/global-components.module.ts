import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UploadComponentComponent } from './upload/upload-component/upload-component.component';
import { MainHeaderComponent } from './layout/main-header/main-header.component';
import { ModalWrapperComponent } from './layout/modal-wrapper/modal-wrapper.component';
import { MatDialogModule } from '@angular/material/dialog';
import { PathSearchInputControlComponent } from './form-controls/path-search-input-control/path-search-input-control.component';
import { RouterModule } from '@angular/router';
import { HighlightPipe } from './pipes/highlight.pipe';
import { ReactiveFormsModule } from '@angular/forms';
import { ErrorMessageComponent } from './messages/error-message/error-message.component';
import { DocumentsSearchInputControlComponent } from './form-controls/documents-search-input-control/documents-search-input-control.component';
import { InputFormControlComponent } from './form-controls/input-form-control/input-form-control.component';
import { ConfirmActionMessageComponent } from './messages/confirm-action-message/confirm-action-message.component';
import { UnauthorizedPageComponent } from './common/unauthorized-page/unauthorized-page.component';
import { MainPageComponent } from './common/main-page/main-page.component';
import { NotFoundPageComponent } from './common/not-found-page/not-found-page.component';
import { AppCoreModule } from '../../app-core/app-core.module';

const material = [MatDialogModule];

const components = [
  UploadComponentComponent,
  MainHeaderComponent,
  ModalWrapperComponent,
  HighlightPipe,
  ErrorMessageComponent,
  ConfirmActionMessageComponent,
];

const formcontrols = [
  PathSearchInputControlComponent,
  DocumentsSearchInputControlComponent,
  InputFormControlComponent,
];

@NgModule({
  declarations: [
    ...components,
    ...formcontrols,
    UnauthorizedPageComponent,
    MainPageComponent,
    NotFoundPageComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AppCoreModule,
    ...material,
    RouterModule,
  ],
  exports: [...components, ...formcontrols],
})
export class GlobalComponentsModule {}
