import { DocumentAccessTypeEnum } from '../enums/document-access-type.enum';

export type PathAccessTypeRequest = {
  accessType: DocumentAccessTypeEnum;
  permissions: string[];
};
