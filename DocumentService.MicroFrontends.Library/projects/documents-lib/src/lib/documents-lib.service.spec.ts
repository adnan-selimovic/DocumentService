import { TestBed } from '@angular/core/testing';

import { DocumentsLibService } from './documents-lib.service';

describe('DocumentsLibService', () => {
  let service: DocumentsLibService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DocumentsLibService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
