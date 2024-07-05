import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class TableService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private baseUrl = 'http://localhost:5127/api/Entity';

  createTable(tableName: string, attributes: {name: string, dataType: string}[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/CreateTable`, {
      tableName,
      attributes,
      createdById: this.authService.getUserId()
    });
  }

  getTables(): Observable<any> {
    return this.http.get(`${this.baseUrl}/GetTables`);
  }
}
