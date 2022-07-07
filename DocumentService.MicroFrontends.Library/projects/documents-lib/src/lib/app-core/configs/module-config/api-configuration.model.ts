export type ApiConfigurationModel = {
  webAppUrl?: string;
  documentServiceUrl?: string;
  authServiceUrl?: string;
  authConfig?: { key: string; value: string }[];
};
