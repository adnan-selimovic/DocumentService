import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentsLibComponent } from './documents-lib.component';

describe('DocumentsLibComponent', () => {
  let component: DocumentsLibComponent;
  let fixture: ComponentFixture<DocumentsLibComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DocumentsLibComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DocumentsLibComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
