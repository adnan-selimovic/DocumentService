import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DocumentOverviewLayoutComponent } from './layout/document-overview-layout/document-overview-layout.component';
import { DocumentsLayoutComponent } from './layout/documents-layout/documents-layout.component';
import { MainDocumentLayoutComponent } from './layout/main-document-layout/main-document-layout.component';
import { PublicDocumentsLayoutComponent } from './layout/public-documents-layout/public-documents-layout.component';
import { SearchDocumentsLayoutComponent } from './layout/search-documents-layout/search-documents-layout.component';
import { UserPermissionsLayoutComponent } from './layout/user-permissions-layout/user-permissions-layout.component';
import { LoadDocumentsResolver } from './resolvers/load-documents.resolver';
import { LoadPublicDocumentsResolver } from './resolvers/load-public-documents.resolver';
import { LoadUserPermissionsResolver } from './resolvers/load-user-permissions.resolver';
import { SelectedDocumentResolver } from './resolvers/selected-document.resolver';

const routes: Routes = [
  {
    path: 'public',
    resolve: {
      documents: LoadPublicDocumentsResolver,
    },
    component: PublicDocumentsLayoutComponent,
  },
  {
    path: '',
    resolve: {
      documents: LoadDocumentsResolver,
    },
    children: [
      {
        path: '',
        component: MainDocumentLayoutComponent,
        children: [
          { path: '', component: DocumentsLayoutComponent },
          {
            path: 'file/:fileId',
            pathMatch: 'full',
            resolve: { selectedDocument: SelectedDocumentResolver },
            component: DocumentOverviewLayoutComponent,
          },
        ],
      },
      {
        path: 'search',
        pathMatch: 'full',
        component: SearchDocumentsLayoutComponent,
      },
      {
        path: 'permissions',
        pathMatch: 'full',
        resolve: {
          permissions: LoadUserPermissionsResolver,
        },
        component: UserPermissionsLayoutComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DocumentsRoutingModule {}
