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

@Component({
  selector: 'app-input-form-control',
  templateUrl: './input-form-control.component.html',
  styleUrls: ['./input-form-control.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputFormControlComponent),
      multi: true,
    },
  ],
})
export class InputFormControlComponent
  implements ControlValueAccessor, OnInit, OnChanges
{
  form: FormGroup = this.fb.group({
    searchInput: [],
  });
  get searchInput() {
    return this.form.get('searchInput');
  }

  @Input() placeholder = '';
  @Input() leftIcon!: string;
  @Input() prefixText?: string;

  value!: string;
  tempValue = '';
  disabled = false;
  selectExpanded = false;

  editing = false;
  @ViewChild('inputContainer', { static: false }) inputContainer!: ElementRef;
  @ViewChild('input', { static: false }) input!: ElementRef;

  onChange: any = () => {};

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {}
  ngOnChanges(changes: SimpleChanges): void {}

  writeValue(value: string): void {
    if (value) {
      this.tempValue = value;
      this.searchInput?.setValue(value);
    }
  }
  registerOnChange(fn: any): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: any): void {}

  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onTempValueChange(event: any) {
    const value = event.target.value;
    this.value = value;
    this.tempValue = value;
    this.writeValue(value);
    this.onChange(value);
  }

  onSearch() {
    if (this.tempValue && this.tempValue !== '') {
      this.writeValue(this.tempValue);
      this.onChange(this.tempValue);
      this.selectExpanded = false;
    }
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
    } else {
      this.value = '';
      this.editing = false;
      this.selectExpanded = false;
    }
  }
}
