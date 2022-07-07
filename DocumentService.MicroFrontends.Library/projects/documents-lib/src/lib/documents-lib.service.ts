import { Inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { ApiConfigurationModel } from './app-core/configs/module-config/api-configuration.model';
import { ModuleConfigModel } from './app-core/configs/module-config/module-config.model';

@Injectable({
  providedIn: 'root',
})
export class DocumentsLibService {
  public apiConfiguration!: ApiConfigurationModel;
  public webAppUrl = environment.webAppUrl;
  public documentServiceUrl = environment.documentServiceUrl;
  public authServiceUrl = environment.authServiceUrl;

  constructor(
    @Inject('moduleConfiguration')
    private moduleConfiguration: ModuleConfigModel
  ) {
    this.apiConfiguration = this.moduleConfiguration.apiConfiguration;
    this.apiConfiguration?.webAppUrl &&
      (this.webAppUrl = this.apiConfiguration.webAppUrl);
    this.apiConfiguration?.authServiceUrl &&
      (this.authServiceUrl = this.apiConfiguration.authServiceUrl);
    this.apiConfiguration?.documentServiceUrl &&
      (this.documentServiceUrl = this.apiConfiguration.documentServiceUrl);
  }
}
