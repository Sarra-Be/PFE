import { ValidationRequestService } from './../../../services/validation-request.service';
import { Component, HostBinding, OnInit, inject } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { DateFnsModule } from 'ngx-date-fns';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-validation-requests',
  standalone: true,
  imports: [DateFnsModule, NgClass],
  templateUrl: './validation-requests.component.html',
  styleUrl: './validation-requests.component.css'
})
export class ValidationRequestsComponent implements OnInit {
  private validationRequestService = inject(ValidationRequestService);
  private toastr = inject(ToastrService);
  authService = inject(AuthService);

  @HostBinding('class') get _classes(): string {
    return 'w-full';
  }

  validationRequests: any[] = [];

  ngOnInit(): void {
    this.getValidationRequests();
  }

  getValidationRequests(): void {
    if (!this.authService.isAdmin()) {
      this.validationRequestService.getValidationRequestsByUserId(this.authService.getUserId())
        .subscribe(data => {
          this.validationRequests = data.map((d:any) => ({...d, creationDate: new Date(d.creationDate) })).reverse();
        });
    } else {
      this.validationRequestService.getValidationRequests()
        .subscribe(data => {
          this.validationRequests = data.map((d:any) => ({...d, creationDate: new Date(d.creationDate) })).reverse();
        })
    }
  }

  onApproveValidationRequestButtonPressed(validationRequest: any): void {
    this.validationRequestService.approveValidationRequest(validationRequest.id)
      .subscribe(_ => {
        this.toastr.success('The request has been approved succesfully!');
        this.getValidationRequests();
      }, err => {
        this.toastr.error(err.error);
        this.getValidationRequests();
      });
  }

  onRejectValidationRequestButtonPressed(validationRequest: any): void {
    this.validationRequestService.rejectValidationRequest(validationRequest.id)
      .subscribe(_ => {
        this.toastr.success('The request has been rejected succesfully!');
        this.getValidationRequests();
      }, err => {
        this.toastr.error(err.error);
        this.getValidationRequests();
      });

  }
}
