import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'app-text-input',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatInput,
    MatError,
    MatLabel
  ],
  templateUrl: './text-input.component.html',
  styleUrl: './text-input.component.scss'
})
            // TextInputComponent need to implement
export class TextInputComponent implements ControlValueAccessor {

  // use decoratorts instead of signal
  @Input() label: string = '';
  @Input() type: string = 'text';

  // need constructor for the decorators to work
  constructor(@Self() public controlDir: NgControl) {
    this.controlDir.valueAccessor = this; // set the value accessor
  }

  writeValue(obj: any): void {

  }
  registerOnChange(fn: any): void {

  }
  registerOnTouched(fn: any): void {

  }

  get control() {
    return this.controlDir.control as FormControl; // return the control from the directive
  }
}
