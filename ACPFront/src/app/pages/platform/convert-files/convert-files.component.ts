import { Component, HostBinding, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import * as xlsx from 'xlsx';
import { ValidationRequestService } from '../../../services/validation-request.service';
import { TableService } from '../../../services/table.service';
import { AuthService } from '../../../services/auth.service';
import { ToastrService } from 'ngx-toastr';

const TABLES_TO_IGNORE = [
  '__EFMigrationsHistory', 'ValidationRequests', 'AspNetRoles', 'AspNetUsers', 'ConversionHistory', 'ActionLogs', 'UploadedFiles', 'AspNetRoleClaims', 'AspNetUserClaims', 'AspNetUserLogins', 'AspNetUserRoles', 'AspNetUserTokens', 'ChatMessages'
]

function getLatestDateObject(objects: any) {
  if (!Array.isArray(objects) || objects.length === 0) {
      return null; // Return null if the input is not an array or is empty
  }

  // Initialize variables to store the latest date and the corresponding object
  let latestDate = new Date(0); // Start with the minimum possible date
  let latestObject = null;

  // Iterate through each object in the array
  objects.forEach(obj => {
      // Assuming the date property is named "date"
      const date = new Date(obj.creationDate);

      // Compare the date with the latestDate found so far
      if (date > latestDate) {
          latestDate = date;
          latestObject = obj;
      }
  });

  return latestObject;
}

@Component({
  selector: 'app-convert-files',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './convert-files.component.html',
  styleUrl: './convert-files.component.css'
})
export class ConvertFilesComponent {
  private validationRequestService = inject(ValidationRequestService);
  private tableService = inject(TableService);
  private toastr = inject(ToastrService);
  authService = inject(AuthService);

  @HostBinding('class') get _classes(): string {
    return 'w-full';
  }

  formGroup = new FormGroup({
    file: new FormControl('', [Validators.required])
  });

  fileName: string | undefined;
  fileContent: any[] | undefined = undefined;
  fileColumns: string[] = [];
  fileValidationHistory: any | undefined = undefined;

  tablesList: any[] = [];

  stepCount = 1;

  selectedTargetTableFormControl = new FormControl('', [Validators.required]);
  targetColumnsMapping: any = {}

  onFileSelected(event: any): void {
    let file = event.target.files[0];

    if (!file) {
      this.formGroup.reset();
      this.fileName = undefined;
      this.fileContent = undefined;
      this.targetColumnsMapping = {};
      this.fileValidationHistory = undefined;
      this.fileColumns = [];
      return;
    }

    if (!file.name.endsWith('.xlsx') && !file.name.endsWith('.xls')) {
      this.formGroup.reset();
      this.fileName = undefined;
      this.fileContent = undefined;
      this.targetColumnsMapping = {};
      this.fileValidationHistory = undefined;
      this.fileColumns = [];
      alert('You can only use excel files!');
      return;
    }

    let fileReader = new FileReader();
    fileReader.readAsBinaryString(file);

    fileReader.onload = (e) => {
      let workBook = xlsx.read(fileReader.result, { type: 'binary' });
      this.fileName = file.name;
      const firstSheetName = workBook.SheetNames[0];
      const el = xlsx.utils.sheet_to_json(workBook.Sheets[firstSheetName]);
      this.fileContent = el;
      this.fileColumns = Object.keys(this.fileContent[0]);
    }
  }

  onGoToStep2ButtonPressed(): void {
    this.validationRequestService.getValidationRequestsByUserId(this.authService.getUserId()).subscribe(data => {
      const sameFileHistory = data.filter((d: any) => d.fileName == this.fileName!);

      if (sameFileHistory && sameFileHistory.length) {
        this.fileValidationHistory = getLatestDateObject(sameFileHistory.map((h: any) => ({ ...h, creationDate: new Date(h.creationDate) })));
      }

      this.stepCount = 2;
    })
  }

  onGoToStep3ButtonPressed(keepOldMapping = false): void {
    if (keepOldMapping) {
      this.selectedTargetTableFormControl.setValue(this.fileValidationHistory.targetTableName);
      this.targetColumnsMapping = JSON.parse(this.fileValidationHistory.attributeMappingStr);
      this.onCreateValidationRequestButtonPressed();
    } else {
      this.tableService.getTables().subscribe(data => {
        this.tablesList = data.filter((table: any) => !TABLES_TO_IGNORE.includes(table.name));
        this.stepCount = 3;
      })
    }
  }

  getSelectedTableColumn(): string[] {
    return this.tablesList.filter(table => table.name == this.selectedTargetTableFormControl.value!)[0].attributes.filter((attribute: any) => attribute != 'Id');
  }

  isCreateValidationButtonDisabled(): boolean {
    const selectedDestinationTableColumns = Object.keys(this.targetColumnsMapping);
    for (let tableColumn of this.getSelectedTableColumn()) {
      if (!selectedDestinationTableColumns.includes(tableColumn)) {
        return true;
      }
    }

    return false;
  }

  onMappingChanged(event: any, tableColumn: string): void {
    this.targetColumnsMapping[tableColumn] = event.target.value;
  }

  onCreateValidationRequestButtonPressed(): void {
    this.validationRequestService.createValidationRequest(this.fileName!,
      this.selectedTargetTableFormControl.value!,
      JSON.stringify(this.targetColumnsMapping),
      JSON.stringify(this.fileContent),
      this.authService.getUserId()
    ).subscribe(_ => {
      if (this.authService.isAdmin()) {
        this.toastr.success('Your file has been converted successfully!');
      } else {
        this.toastr.success('A validation requested has been sent to your administrator!');
      }

      this.stepCount = 4;
    }, err => {
      this.toastr.error(err.error);
    });
  }

  onGoToStep1ButtonPressed(): void {
    this.formGroup.reset();
    this.fileName = undefined;
    this.fileContent = undefined;
    this.targetColumnsMapping = {};
    this.fileValidationHistory = undefined;
    this.fileColumns = [];
    this.stepCount = 1;
  }
}
