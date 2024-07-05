import { Component, HostBinding, inject } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import * as xlsx from 'xlsx';
import { FilePredictionService } from '../../../services/file-prediction.service';
import { ToastrService } from 'ngx-toastr';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-predict-from-files',
  standalone: true,
  imports: [ReactiveFormsModule, NgClass],
  templateUrl: './predict-from-files.component.html',
  styleUrl: './predict-from-files.component.css'
})
export class PredictFromFilesComponent {
  private readonly filePredictionService = inject(FilePredictionService);
  private readonly toastr = inject(ToastrService);

  @HostBinding('class') get _classes(): string {
    return 'w-full';
  }

  formGroup = new FormGroup({
    file: new FormControl('', [Validators.required])
  });

  file: File | undefined;
  fileColumns: string[] = [];
  fileContent = [];

  stepCount = 1;

  onFileSelected(event: any): void {
    this.file = event.target.files[0];

    if (!this.file) {
      this.formGroup.reset();
      return;
    }

    if (!this.file.name.endsWith('.xlsx') && !this.file.name.endsWith('.xls')) {
      this.formGroup.reset();
      alert('You can only use excel files!');
      return;
    }
  }

  onPredictButtonPressed(): void {
    if (this.file) {
      this.filePredictionService.predictFile(this.file).subscribe(
        data => {
          this.toastr.success('File prediction has been performed successfully!');
          this.fileColumns = Object.keys(data[0]);
          this.fileContent = data;
          this.stepCount = 2;
        },
        error => {
          this.toastr.error('There was an error while uploading the file, please try again later!');
          this.file = undefined;
          this.fileColumns = [];
          this.fileContent = [];
          this.stepCount = 1;
        }
      );
    }
  }

  onGoToStep1ButtonPressed(): void {
    this.file = undefined;
    this.fileColumns = [];
    this.fileContent = [];
    this.stepCount = 1;
  }
}
