import { AfterViewInit, Component, ElementRef, HostBinding, OnInit, ViewChild, inject } from '@angular/core';
import { Modal, initFlowbite } from 'flowbite';
import { UserService } from '../../../services/user.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [ReactiveFormsModule, NgClass],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent implements OnInit {
  private userService = inject(UserService);
  private toast = inject(ToastrService);


  @HostBinding('class') get _classes(): string {
    return 'w-full';
  }

  @ViewChild('editModal') editModal!: ElementRef<HTMLElement>;

  usersList: any = [];

  formGroup = new FormGroup({
    firstName: new FormControl('', [Validators.required]),
    lastName: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.email, Validators.required]),
    phoneNumber: new FormControl('', [Validators.required]),
  });

  selectedUserId = '';

  ngOnInit(): void {
    this.getUsers();
  }

  onEditUserButtonPressed(user: any): void {
    const modal = new Modal(this.editModal.nativeElement);
    this.selectedUserId = user.id;
    this.formGroup.patchValue({
      firstName: user.fullName.split(' ')[0],
      lastName: user.fullName.split(' ')[1],
      email: user.email,
      phoneNumber: user.phoneNumber
    });
    modal.show();
  }

  getUsers(): void {
    this.userService.getUsers().subscribe(data => {
      this.usersList = data.filter((user: any) => user.userName !== 'admin');
    })
  }

  onConfirmUserEditButtonPressed(): void {
    this.userService.updateProfile(this.selectedUserId, this.formGroup.value.firstName! + ' ' + this.formGroup.value.lastName!,
      this.formGroup.value.email!, this.formGroup.value.phoneNumber!
    ).subscribe(_ => {
      this.toast.success('Account updated successfully!');
      const modal = new Modal(this.editModal.nativeElement);
      modal.hide();
      this.getUsers();
    }, err => {
      this.toast.error('Error occurred: ' + err.error.message);
    })
  }

  onCloseModalButtonPressed(): void {
    const modal = new Modal(this.editModal.nativeElement);
    modal.hide();
  }
  
  onEnableUserButtonPressed(user: any): void {
    this.userService.activateAccount(user.id).subscribe(_ => {
      this.toast.success('User activated successfully');
      this.getUsers();
    });
  }
  
  onDisableUserButtonPressed(user: any): void {
    this.userService.deactivateAccount(user.id).subscribe(_ => {
      this.toast.success('User deactivated successfully');
      this.getUsers();
    });
  }
}
