import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DocumentsLayoutComponent } from './layout/documents-layout/documents-layout.component';
import { DocumentOverviewLayoutComponent } from './layout/document-overview-layout/document-overview-layout.component';
import { DocumentsRoutingModule } from './documents-routing.module';
import { NgxsModule } from '@ngxs/store';
import { DocumentsState } from './store/documents.state';
import { DocumentsListComponent } from './components/documents-list/documents-list.component';
import { DocumentOverviewComponent } from './components/document-overview/document-overview.component';
import { DocumentsIconListComponent } from './components/documents-icon-list/documents-icon-list.component';
import { NgxDocViewerModule } from 'ngx-doc-viewer';
import { MainDocumentLayoutComponent } from './layout/main-document-layout/main-document-layout.component';
import { FoldersTreeHierarchyComponent } from './components/folders-tree-hierarchy/folders-tree-hierarchy.component';
import { DocumentBreadcrumbHeaderComponent } from './components/document-breadcrumb-header/document-breadcrumb-header.component';
import { CreateNewFolderModalComponent } from './components/create-new-folder-modal/create-new-folder-modal.component';
import { MatDialogModule } from '@angular/material/dialog';
import { ReactiveFormsModule } from '@angular/forms';
import { SearchDocumentsLayoutComponent } from './layout/search-documents-layout/search-documents-layout.component';
import { AppCoreModule } from '../../app-core/app-core.module';
import { GlobalComponentsModule } from '../../shared/global-components/global-components.module';
import { UserPermissionsLayoutComponent } from './layout/user-permissions-layout/user-permissions-layout.component';
import { QuillModule } from 'ngx-quill';
import { FileSaverModule } from 'ngx-filesaver';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { PublicDocumentsLayoutComponent } from './layout/public-documents-layout/public-documents-layout.component';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { CodeEditorModule } from '@ngstack/code-editor';

const material = [MatDialogModule, ClipboardModule, MatSnackBarModule];

@NgModule({
  declarations: [
    DocumentsLayoutComponent,
    DocumentOverviewLayoutComponent,
    DocumentsListComponent,
    DocumentOverviewComponent,
    DocumentsIconListComponent,
    MainDocumentLayoutComponent,
    FoldersTreeHierarchyComponent,
    DocumentBreadcrumbHeaderComponent,
    CreateNewFolderModalComponent,
    SearchDocumentsLayoutComponent,
    UserPermissionsLayoutComponent,
    PublicDocumentsLayoutComponent,
  ],
  imports: [
    CommonModule,
    DocumentsRoutingModule,
    AppCoreModule,
    NgxsModule.forFeature([DocumentsState]),
    NgxDocViewerModule,

    GlobalComponentsModule,
    ...material,
    ReactiveFormsModule,

    QuillModule.forRoot(),
    FileSaverModule,
    CodeEditorModule.forChild(),
  ],
})
export class DocumentsModule {}
