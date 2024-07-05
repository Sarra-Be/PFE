import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LogService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5127/api/ActionLog'

  getLogs(): Observable<any> {
    return this.http.get(`${this.baseUrl}/All`);
  }
}
