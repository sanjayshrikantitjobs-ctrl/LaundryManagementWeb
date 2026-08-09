import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UploadedImage {
  url: string;
}

@Injectable({ providedIn: 'root' })
export class UploadService {
  constructor(private http: HttpClient) {}

  uploadImage(file: File): Observable<UploadedImage> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<UploadedImage>('/api/v1/uploads/images', formData);
  }
}
