import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DialogData } from '../../../../app-core/models/types/dialog-data.type';

@Component({
  selector: 'app-confirm-action-message',
  templateUrl: './confirm-action-message.component.html',
  styleUrls: ['./confirm-action-message.component.scss'],
})
export class ConfirmActionMessageComponent implements OnInit {
  constructor(@Inject(MAT_DIALOG_DATA) public data: DialogData) {}

  ngOnInit(): void {}
}
