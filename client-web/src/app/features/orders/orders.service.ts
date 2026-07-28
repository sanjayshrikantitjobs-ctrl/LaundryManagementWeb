import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateOrderRequest, OrderListItem, OrderStatus, PaginatedList } from '../../core/models/order.models';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly baseUrl = '/api/v1/orders';

  constructor(private http: HttpClient) {}

  getOrders(opts: {
    status?: OrderStatus;
    search?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Observable<PaginatedList<OrderListItem>> {
    let params = new HttpParams()
      .set('pageNumber', opts.pageNumber ?? 1)
      .set('pageSize', opts.pageSize ?? 20);

    if (opts.status !== undefined) params = params.set('status', opts.status);
    if (opts.search) params = params.set('search', opts.search);

    return this.http.get<PaginatedList<OrderListItem>>(this.baseUrl, { params });
  }

  createOrder(request: CreateOrderRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  advanceStatus(orderId: string, newStatus: OrderStatus): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${orderId}/status`, newStatus);
  }
}
