import {
  Component,
  EventEmitter,
  Inject,
  Input,
  OnInit,
  Output,
} from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DialogData } from '../../../../app-core/models/types/dialog-data.type';

@Component({
  selector: 'app-modal-wrapper',
  templateUrl: './modal-wrapper.component.html',
  styleUrls: ['./modal-wrapper.component.scss'],
})
export class ModalWrapperComponent implements OnInit {
  @Output() onConfirmEvent = new EventEmitter();
  @Output() onCloseEvent = new EventEmitter();
  @Input() disabled = false;

  showConfirm = false;
  showClose = true;

  constructor(@Inject(MAT_DIALOG_DATA) public data: DialogData) {
    this.showConfirm = data.showConfirm || false;
    this.showClose = data.showClose || true;
  }

  ngOnInit(): void {}

  onConfirm(): void {
    this.onConfirmEvent.emit();
  }
  onClose(): void {
    this.onCloseEvent.emit();
  }
}
