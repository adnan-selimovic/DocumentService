import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FlexLayoutModule } from '@angular/flex-layout';
import { HttpRequestInterceptor } from './interceptor/http.interceptor';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressBarModule } from '@angular/material/progress-bar';

const material = [MatTooltipModule, MatMenuModule, MatProgressBarModule];

@NgModule({
  declarations: [],
  imports: [CommonModule, FlexLayoutModule, ...material],
  exports: [FlexLayoutModule, ...material],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpRequestInterceptor,
      multi: true,
    },
  ],
})
export class AppCoreModule {}
