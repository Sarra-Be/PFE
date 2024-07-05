import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Observable, map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient)
  private baseUrl = 'http://localhost:5127/api/ApplicationUser';
  private readonly jwt = new JwtHelperService();

  signUp(userName: string, email: string, password: string, fullName: string, phoneNumber: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/Register`, {
      userName,
      email,
      password,
      fullName,
      phoneNumber,
      role: "USER"
    });
  }

  signIn(userName: string, password: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/Login`, {
      userName,
      password
    }).pipe(map((res: any)  => {
      // post-traitement, on le lance après avoir reçu une réponse du backend
      localStorage.setItem('token', res.token);
      return res;
    }));
  }

  getToken(): string | null {
    return localStorage.getItem('token') ?? null;
  }

  getUserId(): string {
    return this.getTokenDetails()['UserID'];
  }

  getUserRole(): string {
    const token = this.getToken();

    if (!token) {
      throw Error('Token does not exist');
    }
    const parsedToken = this.jwt.decodeToken(token);
    return parsedToken['role'];
  }

  getUserRoleStr(): string {
    const role = this.getUserRole();
    if (role == 'ADMIN') {
      return 'Admin'
    } else {
      return 'Consultant';
    }
  }

  getTokenDetails(): any {
    const token = this.getToken();

    if (!token) {
      throw Error('Token does not exist');
    }

    return this.jwt.decodeToken(token);
  }

  isAdmin(): boolean {
    return this.getUserRole() == 'ADMIN';
  }

  isUser(): boolean {
    return this.getUserRole() == 'USER';
  }

  logout(): void {
    localStorage.removeItem('token');
  }

  requestPasswordReset(userName: string) {
    return this.http.post(`${this.baseUrl}/RequestPasswordReset/${userName}`, {});
  }

  resetPassword(token: string, password: string) {
    return this.http.post(`${this.baseUrl}/ResetPassword`, {
      token,
      newPassword: password
    });
  }
}
