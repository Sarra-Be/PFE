import { AuthService } from './../../services/auth.service';
import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { initFlowbite } from 'flowbite';
import { ToastrService } from 'ngx-toastr';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-platform',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './platform.component.html',
  styleUrl: './platform.component.css'
})
export class PlatformComponent implements OnInit {
  private router = inject(Router);
  private toast = inject(ToastrService);
  private userService = inject(UserService);
  authService = inject(AuthService);

  userInfo: any;

  ngOnInit(): void {
    initFlowbite();

    this.userService.getUserProfile().subscribe(data => {
      this.userInfo = data;
    })
  }

  onLogoutButtonPressed(): void {
    this.authService.logout();
    this.router.navigateByUrl('/auth/sign-in');
    this.toast.success('Logged out successfully!');
  }
}
