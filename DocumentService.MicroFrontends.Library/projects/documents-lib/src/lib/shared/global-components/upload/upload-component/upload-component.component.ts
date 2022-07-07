import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  OnInit,
  Output,
} from '@angular/core';

@Component({
  selector: 'app-upload-component',
  templateUrl: './upload-component.component.html',
  styleUrls: ['./upload-component.component.scss'],
})
export class UploadComponentComponent implements OnInit {
  DOCUMENTS: any[] = [];
  @Input() disabled = false;
  @Input() showDropzone = false;
  @Input() multipleUpload = false;
  @Output() getDocuments: EventEmitter<any> = new EventEmitter<any>();
  url: any;
  dropzoneHovered = false;

  ngOnInit(): void {}

  onClick(event: any) {
    event.preventDefault();
    event.stopPropagation();
    if (this.disabled) {
      return;
    }
    const files = event.target.files;
    this.addDcouments(files);
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    if (this.disabled) {
      return;
    }
    let files = event.dataTransfer?.files;
    this.addDcouments(files);
    this.dropzoneHovered = false;
  }

  onDragEnter(event: any) {
    event.preventDefault();
    event.stopPropagation();
    if (this.disabled) {
      return;
    }
    this.dropzoneHovered = true;
  }
  onDragLeave(event: any) {
    event.preventDefault();
    event.stopPropagation();
    if (this.disabled) {
      return;
    }
    this.dropzoneHovered = false;
  }

  addDcouments(files: any) {
    if (!this.multipleUpload && files.length > 1) {
      return;
    }

    for (let i = 0; i < files.length; i++) {
      let doc = files[i];
      this.DOCUMENTS.push(doc);

      const reader = new FileReader();
      reader.onload = (e) => {
        return (this.url = reader.result);
      };
      reader.readAsDataURL(files[i]);
    }
    this.getDocuments.emit(this.DOCUMENTS);
    this.DOCUMENTS = [];
  }
  deleteDocumentOnIndex(index: number) {
    this.DOCUMENTS.splice(index, 1);
  }

  @HostListener('dragover', ['$event']) public dragover(evt: DragEvent) {
    evt.preventDefault();
    evt.stopPropagation();
  }
}
