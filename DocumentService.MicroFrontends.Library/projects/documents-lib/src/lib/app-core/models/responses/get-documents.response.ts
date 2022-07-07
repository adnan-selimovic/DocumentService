import { DocumentAccessTypeEnum } from '../enums/document-access-type.enum';
import { TDocument } from '../types/document.type';
import { TFolder } from '../types/folder.type';

export class RDocuments {
  folders!: TFolder[];
  documents!: TDocument[];
  rootFolderPath?: string;
  currentFolder?: TFolder | null;
  isEmpty = false;
  permissions?: string[];
  accessType!: DocumentAccessTypeEnum;
}
