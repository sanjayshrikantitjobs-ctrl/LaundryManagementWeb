import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateContactMessageRequest, MyContactMessage } from '../../core/models/contact-message.models';

@Injectable({ providedIn: 'root' })
export class ContactMessagesService {
  private readonly baseUrl = '/api/v1/contactmessages';

  constructor(private http: HttpClient) {}

  getMine(): Observable<MyContactMessage[]> {
    return this.http.get<MyContactMessage[]>(`${this.baseUrl}/mine`);
  }

  create(request: CreateContactMessageRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }
}
