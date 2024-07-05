import { Component, HostBinding, inject } from '@angular/core';
import { TableService } from '../../../services/table.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-create-new-template',
  standalone: true,
  imports: [ReactiveFormsModule, NgClass],
  templateUrl: './create-new-template.component.html',
  styleUrl: './create-new-template.component.css'
})
export class CreateNewTemplateComponent {
  private tableService = inject(TableService);
  private toastr = inject(ToastrService);

  attributesList: any = [];
  tableNameFormControl = new FormControl('', [Validators.required])

  addAttributeFormGroup = new FormGroup({
    attributeName: new FormControl('', [Validators.required, Validators.pattern(/^(?!Id$)[A-Z][a-zA-Z0-9_]*$/), Validators.minLength(3)]),
    attributeType: new FormControl('', [Validators.required]),
  })

  @HostBinding('class') get _classes(): string {
    return 'w-full';
  }

  onAddAttributeButtonPressed(): void {
    this.attributesList.push({
      name: this.addAttributeFormGroup.value.attributeName,
      type: this.addAttributeFormGroup.value.attributeType,
    });

    this.addAttributeFormGroup.reset();
  }

  onDeleteAttributeButtonPressed(attributeIdx: number): void {
    let newList = [];
    for(let i = 0; i < this.attributesList.length; i++) {
      if (i !== attributeIdx) {
        newList.push(this.attributesList[i]);
      }
    }
    this.attributesList = newList;
  }

  onCreateTableButtonPressed(): void {
    const attributes = this.attributesList.map((attribute: any) => ({
      name: attribute.name,
      dataType: attribute.type
    }))
    this.tableService.createTable(this.tableNameFormControl.value!, attributes)
      .subscribe(_ => {
        this.toastr.success('Table created successfully!');
        this.tableNameFormControl.reset();
        this.addAttributeFormGroup.reset();
        this.attributesList = [];
      }, err => {
        this.toastr.error(err.error.message);
      });
  }
}
