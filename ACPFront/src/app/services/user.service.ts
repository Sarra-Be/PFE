import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private baseUserUrl = 'http://localhost:5127/api/ApplicationUser';
  private baseProfileUrl = 'http://localhost:5127/api/UserProfile';

  getUsers(): Observable<any> {
    return this.http.get(`${this.baseUserUrl}/GetUsers`);
  }

  updateProfile(userId: string, fullName: string, email: string, phoneNumber: string): Observable<any> {
    return this.http.put(`${this.baseUserUrl}/UpdateUser/${userId}?updatedBy=${this.authService.getUserId()}`, {
      fullName,
      email,
      phoneNumber,

    });
  }

  getUserProfile(): Observable<any> {
    return this.http.get(`${this.baseProfileUrl}`);
  }

  deactivateAccount(userId: string): Observable<any> {
    return this.http.put(`${this.baseUserUrl}/DeactivateAccount/${userId}`, {});
  }

  activateAccount(userId: string): Observable<any> {
    return this.http.put(`${this.baseUserUrl}/ActivateAccount/${userId}`, {});
  }
}
