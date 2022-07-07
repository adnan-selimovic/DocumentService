import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthResolver } from './app-core/resolvers/auth.resolver';
import { MainPageComponent } from './shared/global-components/common/main-page/main-page.component';
import { NotFoundPageComponent } from './shared/global-components/common/not-found-page/not-found-page.component';
import { UnauthorizedPageComponent } from './shared/global-components/common/unauthorized-page/unauthorized-page.component';

const routes: Routes = [
  {
    path: '',
    resolve: [AuthResolver],
    component: MainPageComponent,
    loadChildren: () =>
      import('./pages/pages.module').then((m) => m.PagesModule),
  },
  {
    path: 'unauthorized',
    component: UnauthorizedPageComponent,
  },
  {
    path: 'not-found',
    component: NotFoundPageComponent,
  },
  { path: '**', redirectTo: '/not-found' },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DocumentsLibRoutingModule {}
