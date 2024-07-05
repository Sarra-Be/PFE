import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FilePredictionService {
  private http = inject(HttpClient);
  private baseUrl = 'http://127.0.0.1:5000'

  predictFile(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post(`${this.baseUrl}/predict`, formData);
  }
}
