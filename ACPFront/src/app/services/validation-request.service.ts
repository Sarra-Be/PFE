import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class ValidationRequestService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private baseUrl = 'http://localhost:5127/api/ValidationRequest';

  getValidationRequestsByUserId(userId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/User/${userId}`);
  }

  getValidationRequests(): Observable<any> {
    return this.http.get(`${this.baseUrl}/All`);
  }

  approveValidationRequest(requestId: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/${requestId}/Approve`, {});
  }

  rejectValidationRequest(requestId: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/${requestId}/Reject`, {});
  }

  createValidationRequest(fileName: string, targetTableName: string,
    attributeMappingStr: string,
    fileJsonStrContent: string,
    requestById: string,
  ): Observable<any> {
    return this.http.post(`${this.baseUrl}`, {
      fileName,
      targetTableName,
      attributeMappingStr,
      fileJsonStrContent,
      requestById,
      isAdmin: this.authService.isAdmin()
    });
  }
}
