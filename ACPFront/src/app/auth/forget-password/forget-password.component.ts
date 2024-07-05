import { NgClass } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-forget-password',
  standalone: true,
  imports: [ReactiveFormsModule, NgClass],
  templateUrl: './forget-password.component.html',
  styleUrl: './forget-password.component.css'
})
export class ForgetPasswordComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastr = inject(ToastrService);

  formGroup = new FormGroup({
    userName: new FormControl('', [Validators.required])
  });


  onConfirmButtonPressed(): void {
    this.authService.requestPasswordReset(this.formGroup.value.userName!)
      .subscribe(_ => {
        this.toastr.success('Password reset was successfully requested!', 'Please have a look at your email inbox for next steps.');
        this.router.navigateByUrl('/auth/sign-in');
      }, err => {
        this.toastr.error(err.error.message);
      })
  }
}
