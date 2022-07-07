import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-document-overview',
  templateUrl: './document-overview.component.html',
  styleUrls: ['./document-overview.component.scss'],
})
export class DocumentOverviewComponent implements OnInit {
  @Input() url!: string;
  @Output() onLoad = new EventEmitter();
  isContentLoaded = false;

  constructor() {}

  ngOnInit(): void {}

  contentLoaded() {
    this.isContentLoaded = true;
    this.onLoad.emit();
  }
}
