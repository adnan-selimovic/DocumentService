import { ErrorResponse } from '../classes/error-response.class';
import { TDocument } from '../types/document.type';

export class UploadDocumentResponse {
  failedDocuments!: ErrorResponse[];
  updatedDocuments!: TDocument[];
  uploadedDocuments!: TDocument[];

  numberOfFailed!: number;
  numberOfUpdates!: number;
  numberOfUploads!: number;
}
