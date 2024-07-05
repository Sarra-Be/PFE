import { Component, HostBinding, OnInit, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { initFlowbite } from 'flowbite';
import { AuthService } from '../../services/auth.service';
import { JwtHelperService } from '@auth0/angular-jwt';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private jwt = inject(JwtHelperService);
  private router = inject(Router);

  @HostBinding('class') get _classes(): string {
    return 'w-full';
  }

  ngOnInit(): void {
    initFlowbite();

    const token = localStorage.getItem('token');

    if (token && !this.jwt.isTokenExpired(token)) {
      this.router.navigateByUrl('/platform');
    }
  }
}
