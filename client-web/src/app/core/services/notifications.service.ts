import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppNotification } from '../models/notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly baseUrl = '/api/v1/notifications';

  constructor(private http: HttpClient) {}

  getMine(take = 30): Observable<AppNotification[]> {
    return this.http.get<AppNotification[]>(`${this.baseUrl}/mine`, { params: { take } });
  }

  getUnreadCount(): Observable<number> {
    return this.http.get<number>(`${this.baseUrl}/mine/unread-count`);
  }

  markRead(id: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/read`, {});
  }
}
