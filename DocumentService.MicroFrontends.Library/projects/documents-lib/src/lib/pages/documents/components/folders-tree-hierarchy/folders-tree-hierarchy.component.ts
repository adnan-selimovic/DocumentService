import { Component, OnInit } from '@angular/core';
import { Select } from '@ngxs/store';
import { expandFolderOnClick } from '../../../../app-core/helpers';
import { FolderPath, FolderHierarchy } from '../../../../app-core/models';
import { Observable } from 'rxjs';
import { DocumentsSelectors } from '../../store/documents.selectors';

@Component({
  selector: 'app-folders-tree-hierarchy',
  templateUrl: './folders-tree-hierarchy.component.html',
  styleUrls: ['./folders-tree-hierarchy.component.scss'],
})
export class FoldersTreeHierarchyComponent implements OnInit {
  @Select(DocumentsSelectors.getFolderPath)
  getFolderPath$!: Observable<FolderPath>;
  getFolderPath!: FolderPath;
  @Select(DocumentsSelectors.getFoldersTree)
  getFoldersTree$!: Observable<FolderHierarchy[]>;
  getFoldersTree!: FolderHierarchy[];

  constructor() {
    this.getFoldersTree$.subscribe((values) => (this.getFoldersTree = values));
    this.getFolderPath$.subscribe((values) => (this.getFolderPath = values));
  }

  expandFolder(path: string) {
    this.getFoldersTree = expandFolderOnClick(this.getFoldersTree, path);
  }

  ngOnInit(): void {}
}
