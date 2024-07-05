import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5127/api/ChatMessages'

  getMessages(): Observable<any> {
    return this.http.get(`${this.baseUrl}`);
  }

  sendMessage(ownerId: string, message: string): Observable<any> {
    return this.http.post(`${this.baseUrl}`, {
      ownerId,
      message
    });
  }
}
