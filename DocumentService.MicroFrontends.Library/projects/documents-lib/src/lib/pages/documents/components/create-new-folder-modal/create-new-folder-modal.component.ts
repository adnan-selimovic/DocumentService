import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FolderPath, DialogData } from '../../../../app-core/models';
import { Select, Store } from '@ngxs/store';
import { CreateFolder } from '../../../documents/store/documents.actions';
import { DocumentsSelectors } from '../../../documents/store/documents.selectors';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-create-new-folder-modal',
  templateUrl: './create-new-folder-modal.component.html',
  styleUrls: ['./create-new-folder-modal.component.scss'],
})
export class CreateNewFolderModalComponent implements OnInit {
  @Select(DocumentsSelectors.getFolderPath)
  getFolderPath$!: Observable<FolderPath>;
  form = this.fb.group({
    folderName: ['', Validators.required],
  });
  get folderName() {
    return this.form.get('folderName');
  }

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private fb: FormBuilder,
    private store: Store,
    private dialogRef: MatDialogRef<any>
  ) {}

  ngOnInit(): void {}

  onSubmitForm(): void {
    this.store
      .dispatch(new CreateFolder(this.folderName?.value))
      .subscribe(() => this.dialogRef.close());
  }
}
