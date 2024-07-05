import { UserService } from './../../../services/user.service';
import { Component, HostBinding, OnInit, inject } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ReactiveFormsModule, NgClass],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private toastr = inject(ToastrService);

  @HostBinding('class') get _classes(): string {
    return 'w-full';
  }

  formGroup = new FormGroup({
    firstName: new FormControl('', [Validators.required]),
    lastName: new FormControl('', [Validators.required]),
    phoneNumber: new FormControl('', [Validators.required, Validators.pattern(/^(09|08|07|05)\d{8}$/)]),
    email: new FormControl('', [Validators.required, Validators.email])
  });

  ngOnInit(): void {
    this.userService.getUserProfile().subscribe(data => {
      this.formGroup.patchValue({
        firstName: data.fullName.split(' ')[0],
        lastName: data.fullName.split(' ')[1],
        phoneNumber: data.phoneNumber,
        email: data.email
      })
    })
  }

  onSaveButtonPressed(): void {
    this.userService.updateProfile(this.authService.getUserId(),
    this.formGroup.value.firstName! + ' ' + this.formGroup.value.lastName!,
     this.formGroup.value.email!, this.formGroup.value.phoneNumber!).subscribe(_ => {
      this.toastr.success('Information updated');
    }, err => {
      this.toastr.error(err.error.message)
    })
  }

}
