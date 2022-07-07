import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DocumentsLibComponent } from 'documents-lib';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: '/documents' },
  {
    path: '',
    component: DocumentsLibComponent,
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
