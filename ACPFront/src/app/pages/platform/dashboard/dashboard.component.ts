import { Component, HostBinding, OnInit, inject } from '@angular/core';
import ApexCharts from 'apexcharts';
import { initFlowbite } from 'flowbite';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';
import { DashboardService } from '../../../services/dashboard.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private dashboardService = inject(DashboardService);
  private router = inject(Router);

  stats: any = {
    totalUsers: 0,
    totalConversions: 0,
    pendingConversions: 0,
    approvedConversions: 0,
    rejectedConversions: 0
  }

  @HostBinding('class') get _classes(): string {
    return 'w-full';
  }

  ngOnInit(): void {
    if (!this.authService.isAdmin()) {
      this.router.navigateByUrl('/platform/profile');
    }

    initFlowbite();

    this.dashboardService.getStats().subscribe((data: any) => {
      this.stats = data;
    })
  }
}
