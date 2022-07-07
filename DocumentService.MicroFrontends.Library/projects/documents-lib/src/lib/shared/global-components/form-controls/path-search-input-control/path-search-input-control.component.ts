import {
  Component,
  ElementRef,
  forwardRef,
  HostListener,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import {
  ControlValueAccessor,
  FormBuilder,
  FormGroup,
  NG_VALUE_ACCESSOR,
} from '@angular/forms';
import { Select, Store } from '@ngxs/store';
import {
  FolderPath,
  QueryFilter,
  PaginationResponse,
  SearchFolderHierarchyModel,
} from '../../../../app-core/models';
import { SearchFoldersHierarchy } from '../../../../pages/documents/store/documents.actions';
import { DocumentsSelectors } from '../../../../pages/documents/store/documents.selectors';
import { debounceTime, Observable } from 'rxjs';

@Component({
  selector: 'app-path-search-input-control',
  templateUrl: './path-search-input-control.component.html',
  styleUrls: ['./path-search-input-control.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PathSearchInputControlComponent),
      multi: true,
    },
  ],
})
export class PathSearchInputControlComponent
  implements ControlValueAccessor, OnInit, OnChanges
{
  @Select(DocumentsSelectors.getFolderPath)
  getFolderPath$!: Observable<FolderPath>;
  @Select(DocumentsSelectors.getSearchFolderHierarchy)
  getSearchFolderHierarchy$!: Observable<
    PaginationResponse<SearchFolderHierarchyModel>
  >;

  form: FormGroup = this.fb.group({
    searchInput: [],
  });
  get searchInput() {
    return this.form.get('searchInput');
  }

  resultSelection: SearchFolderHierarchyModel[] = [];

  @Input() placeholder = '';
  value!: string;
  tempValue = '';
  disabled = false;
  selectExpanded = false;
  queryFilter = new QueryFilter();

  editing = false;
  @ViewChild('inputContainer', { static: false }) inputContainer!: ElementRef;
  @ViewChild('input', { static: false }) input!: ElementRef;

  onChange: any = () => {};

  constructor(private store: Store, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.searchInput?.valueChanges
      .pipe(debounceTime(400))
      .subscribe((value) => this.onInputChange(value));

    this.getSearchFolderHierarchy$.subscribe((values) => {
      if (values && values.items) {
        this.resultSelection = values.items;
      }
    });
  }
  ngOnChanges(changes: SimpleChanges): void {}

  searchFolderHierarchy() {
    if (this.selectExpanded) {
      this.store.dispatch(new SearchFoldersHierarchy(this.queryFilter));
    }
  }

  writeValue(value: string): void {
    this.searchInput?.setValue(value);
  }
  registerOnChange(fn: any): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: any): void {}

  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInputChange(value: any) {
    this.queryFilter.searchTerm = value;
    this.searchFolderHierarchy();
  }
  onTempValueChange(event: any) {
    const value = event.target.value;
    this.value = value;
    this.tempValue = value;
  }

  selectItem(item: any) {
    this.writeValue(item.path_url || item);
    this.onChange(item.path_url || item);
    this.selectExpanded = false;
  }

  // host listeners
  @HostListener('document:click', ['$event'])
  clickout(event: any): void {
    if (
      this.inputContainer &&
      this.inputContainer.nativeElement.contains(event.target)
    ) {
      this.editing = true;
      this.input.nativeElement.focus();
      this.selectExpanded = true;
      this.searchFolderHierarchy();
    } else {
      this.value = '';
      this.editing = false;
      this.selectExpanded = false;
    }
  }
}
