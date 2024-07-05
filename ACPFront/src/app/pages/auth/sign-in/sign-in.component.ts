import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-sign-in',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, FormsModule, CommonModule],
  templateUrl: './sign-in.component.html',
  styleUrl: './sign-in.component.css'
})
export class SignInComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastr = inject(ToastrService);
  private shouldSaveUserName = false;

  formGroup = new FormGroup({
    userName: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required])
  });

  ngOnInit(): void {
    const savedUserName = localStorage.getItem('savedUserName');

    if (savedUserName) {
      this.formGroup.patchValue({
        userName: savedUserName
      });
    }
  }

  onSignInButtonPressed(): void {
    if (this.shouldSaveUserName) {
      localStorage.setItem('savedUserName', this.formGroup.value.userName!);
    }
    this.authService.signIn(this.formGroup.value.userName!, this.formGroup.value.password!)
      .subscribe(_ => {
        this.router.navigateByUrl('/platform');
        this.toastr.success('Logged in successfully!')
      }, err => {
        this.toastr.error(err.error.message)
      })
  }

  onRememberMeCheckChanged(value: any): void {
    this.shouldSaveUserName = value.target.checked;
  }
}
