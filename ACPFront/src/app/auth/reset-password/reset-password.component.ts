import { NgClass } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../services/auth.service';
import { JwtHelperService } from '@auth0/angular-jwt';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, NgClass, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastr = inject(ToastrService);
  private activatedRoute = inject(ActivatedRoute);
  private jwtService = inject(JwtHelperService);

  formGroup = new FormGroup({
    password: new FormControl('', [Validators.required]),
    confirmPassword: new FormControl('', [Validators.required]),
  }, [this.matchValidator('password', 'confirmPassword')]);
  token!: string;

  ngOnInit(): void {
    this.token = this.activatedRoute.snapshot.queryParams['token'];
    if (!this.token) {
      this.toastr.error('No token was found!');
    } else if (this.jwtService.isTokenExpired(this.token)) {
      this.toastr.error('The token has expired, please try resetting your password again!');
      this.router.navigateByUrl('/auth/sign-in');
    }
  }

  onSubmitButtonPressed(): void {
    this.authService.resetPassword(this.token, this.formGroup.value.password!)
      .subscribe(() => {
        this.toastr.success('Password updated successfully!');
        this.router.navigateByUrl('/auth/sign-in');
      }, (err) => {
        this.toastr.error(err.error.message);
      })
  }

  matchValidator(controlName: string, matchingControlName: string): ValidatorFn {
    return (abstractControl: AbstractControl) => {
        const control = abstractControl.get(controlName);
        const matchingControl = abstractControl.get(matchingControlName);

        if (matchingControl!.errors && !matchingControl!.errors?.['confirmedValidator']) {
            return null;
        }

        if (control!.value !== matchingControl!.value) {
          const error = { confirmedValidator: 'Passwords do not match.' };
          matchingControl!.setErrors(error);
          return error;
        } else {
          matchingControl!.setErrors(null);
          return null;
        }
    }
  }
}
