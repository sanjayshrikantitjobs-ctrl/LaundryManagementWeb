import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateOrderRequest,
  OrderDetail,
  OrderListItem,
  OrderStatus,
  PaginatedList,
  SortDirection,
  UpdateOrderRequest
} from '../../core/models/order.models';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly baseUrl = '/api/v1/orders';

  constructor(private http: HttpClient) {}

  getOrders(opts: {
    status?: OrderStatus;
    statuses?: string;
    search?: string;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: SortDirection;
  } = {}): Observable<PaginatedList<OrderListItem>> {
    let params = new HttpParams()
      .set('pageNumber', opts.pageNumber ?? 1)
      .set('pageSize', opts.pageSize ?? 20);

    if (opts.status !== undefined) params = params.set('status', opts.status);
    if (opts.statuses) params = params.set('statuses', opts.statuses);
    if (opts.search) params = params.set('search', opts.search);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDirection) params = params.set('sortDirection', opts.sortDirection);

    return this.http.get<PaginatedList<OrderListItem>>(this.baseUrl, { params });
  }

  getMyOrders(opts: {
    status?: OrderStatus;
    statuses?: string;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: SortDirection;
  } = {}): Observable<PaginatedList<OrderListItem>> {
    let params = new HttpParams()
      .set('pageNumber', opts.pageNumber ?? 1)
      .set('pageSize', opts.pageSize ?? 20);

    if (opts.status !== undefined) params = params.set('status', opts.status);
    if (opts.statuses) params = params.set('statuses', opts.statuses);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDirection) params = params.set('sortDirection', opts.sortDirection);

    return this.http.get<PaginatedList<OrderListItem>>(`${this.baseUrl}/mine`, { params });
  }

  getOrderById(orderId: string): Observable<OrderDetail> {
    return this.http.get<OrderDetail>(`${this.baseUrl}/${orderId}`);
  }

  createOrder(request: CreateOrderRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  advanceStatus(orderId: string, newStatus: OrderStatus): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${orderId}/status`, newStatus);
  }

  updateOrder(orderId: string, request: UpdateOrderRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${orderId}`, request);
  }
}
