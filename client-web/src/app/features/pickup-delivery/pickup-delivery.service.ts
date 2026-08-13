import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Agent, MyPickupDelivery, OrderPickupDelivery } from '../../core/models/pickup-delivery.models';
import { PaginatedList } from '../../core/models/order.models';
import { UserRole } from '../../core/models/user.models';

@Injectable({ providedIn: 'root' })
export class PickupDeliveryService {
  private readonly baseUrl = '/api/v1/pickup-delivery';

  constructor(private http: HttpClient) {}

  getMine(pageNumber = 1, pageSize = 20): Observable<PaginatedList<MyPickupDelivery>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PaginatedList<MyPickupDelivery>>(`${this.baseUrl}/mine`, { params });
  }

  confirmPickup(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/confirm-pickup`, {});
  }

  confirmDelivery(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/confirm-delivery`, {});
  }

  getForOrder(orderId: string): Observable<OrderPickupDelivery[]> {
    return this.http.get<OrderPickupDelivery[]>(`${this.baseUrl}/order/${orderId}`);
  }

  getAgents(role: UserRole): Observable<Agent[]> {
    const params = new HttpParams().set('role', role);
    return this.http.get<Agent[]>(`${this.baseUrl}/agents`, { params });
  }

  assignAgent(id: string, employeeId: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/assign`, { employeeId });
  }
}
