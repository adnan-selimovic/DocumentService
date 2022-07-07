import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ErrorResponse, DialogData } from '../../../../app-core/models';

@Component({
  selector: 'app-error-message',
  templateUrl: './error-message.component.html',
  styleUrls: ['./error-message.component.scss'],
})
export class ErrorMessageComponent implements OnInit {
  errorMessages!: ErrorResponse[];

  constructor(@Inject(MAT_DIALOG_DATA) public dialogData: DialogData) {}

  ngOnInit(): void {
    this.errorMessages = this.dialogData.data as ErrorResponse[];
  }
}
