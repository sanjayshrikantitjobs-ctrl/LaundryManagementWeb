import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateCustomerRequest,
  CustomerDetail,
  CustomerListItem,
  UpdateCustomerRequest
} from '../../core/models/customer.models';
import { PaginatedList } from '../../core/models/order.models';

@Injectable({ providedIn: 'root' })
export class CustomersService {
  private readonly baseUrl = '/api/v1/customers';

  constructor(private http: HttpClient) {}

  getCustomers(opts: { search?: string; pageNumber?: number; pageSize?: number } = {}): Observable<PaginatedList<CustomerListItem>> {
    let params = new HttpParams()
      .set('pageNumber', opts.pageNumber ?? 1)
      .set('pageSize', opts.pageSize ?? 20);

    if (opts.search) params = params.set('search', opts.search);

    return this.http.get<PaginatedList<CustomerListItem>>(this.baseUrl, { params });
  }

  getCustomerById(id: string): Observable<CustomerDetail> {
    return this.http.get<CustomerDetail>(`${this.baseUrl}/${id}`);
  }

  createCustomer(request: CreateCustomerRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  updateCustomer(id: string, request: UpdateCustomerRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  deleteCustomer(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
