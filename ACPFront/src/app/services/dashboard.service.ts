import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5127/api/Dashboard'

  getStats(): Observable<any> {
    return this.http.get(`${this.baseUrl}`);
  }
}
