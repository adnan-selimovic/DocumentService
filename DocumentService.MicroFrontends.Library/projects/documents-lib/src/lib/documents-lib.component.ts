import { Component, OnInit, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'lib-documents-lib',
  template: `<router-outlet></router-outlet>`,
  styles: [],
  encapsulation: ViewEncapsulation.None,
})
export class DocumentsLibComponent implements OnInit {
  constructor() {}

  ngOnInit(): void {}
}
