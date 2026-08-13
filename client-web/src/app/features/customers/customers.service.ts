import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateCustomerRequest,
  CustomerAddress,
  CreateCustomerAddressRequest,
  CustomerDetail,
  CustomerListItem,
  CustomerStatus,
  UpdateCustomerRequest,
  UpdateMyProfileRequest
} from '../../core/models/customer.models';
import { PaginatedList, SortDirection } from '../../core/models/order.models';

@Injectable({ providedIn: 'root' })
export class CustomersService {
  private readonly baseUrl = '/api/v1/customers';

  constructor(private http: HttpClient) {}

  getCustomers(opts: {
    search?: string;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: SortDirection;
    hasOrders?: boolean;
    status?: CustomerStatus;
  } = {}): Observable<PaginatedList<CustomerListItem>> {
    let params = new HttpParams()
      .set('pageNumber', opts.pageNumber ?? 1)
      .set('pageSize', opts.pageSize ?? 20);

    if (opts.search) params = params.set('search', opts.search);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDirection) params = params.set('sortDirection', opts.sortDirection);
    if (opts.hasOrders !== undefined) params = params.set('hasOrders', opts.hasOrders);
    if (opts.status !== undefined) params = params.set('status', opts.status);

    return this.http.get<PaginatedList<CustomerListItem>>(this.baseUrl, { params });
  }

  getCustomerById(id: string): Observable<CustomerDetail> {
    return this.http.get<CustomerDetail>(`${this.baseUrl}/${id}`);
  }

  getMyProfile(): Observable<CustomerListItem> {
    return this.http.get<CustomerListItem>(`${this.baseUrl}/me`);
  }

  updateMyProfile(request: UpdateMyProfileRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/me`, request);
  }

  getMyAddresses(): Observable<CustomerAddress[]> {
    return this.http.get<CustomerAddress[]>(`${this.baseUrl}/me/addresses`);
  }

  addMyAddress(request: CreateCustomerAddressRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/me/addresses`, request);
  }

  setPrimaryAddress(addressId: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/me/addresses/${addressId}/primary`, {});
  }

  deleteMyAddress(addressId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/me/addresses/${addressId}`);
  }

  setPassword(customerId: string, newPassword: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${customerId}/password`, { newPassword });
  }

  createCustomer(request: CreateCustomerRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  updateCustomer(id: string, request: UpdateCustomerRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  deleteCustomer(id: string, reason?: string): Observable<void> {
    let params = new HttpParams();
    if (reason) params = params.set('reason', reason);
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { params });
  }

  setStatus(id: string, status: CustomerStatus): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/status`, { status });
  }
}
