import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateServiceRequest, ServiceDetail, ServiceListItem, UpdateServiceRequest } from '../../core/models/catalog.models';
import { PaginatedList, SortDirection } from '../../core/models/order.models';

@Injectable({ providedIn: 'root' })
export class ServicesService {
  private readonly baseUrl = '/api/v1/services';

  constructor(private http: HttpClient) {}

  getServices(opts: {
    search?: string;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: SortDirection;
  } = {}): Observable<PaginatedList<ServiceListItem>> {
    let params = new HttpParams()
      .set('pageNumber', opts.pageNumber ?? 1)
      .set('pageSize', opts.pageSize ?? 20);

    if (opts.search) params = params.set('search', opts.search);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDirection) params = params.set('sortDirection', opts.sortDirection);

    return this.http.get<PaginatedList<ServiceListItem>>(this.baseUrl, { params });
  }

  getServiceById(id: string): Observable<ServiceDetail> {
    return this.http.get<ServiceDetail>(`${this.baseUrl}/${id}`);
  }

  createService(request: CreateServiceRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  updateService(id: string, request: UpdateServiceRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  deleteService(id: string, reason?: string): Observable<void> {
    let params = new HttpParams();
    if (reason) params = params.set('reason', reason);
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { params });
  }
}
