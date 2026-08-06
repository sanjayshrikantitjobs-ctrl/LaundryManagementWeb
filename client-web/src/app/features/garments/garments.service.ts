import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateGarmentRequest,
  GarmentDetail,
  GarmentListItem,
  PricingMatrix,
  PricingType,
  UpdateGarmentRequest
} from '../../core/models/catalog.models';
import { PaginatedList } from '../../core/models/order.models';

@Injectable({ providedIn: 'root' })
export class GarmentsService {
  private readonly baseUrl = '/api/v1/garments';

  constructor(private http: HttpClient) {}

  getGarments(opts: { search?: string; pageNumber?: number; pageSize?: number } = {}): Observable<PaginatedList<GarmentListItem>> {
    let params = new HttpParams()
      .set('pageNumber', opts.pageNumber ?? 1)
      .set('pageSize', opts.pageSize ?? 20);

    if (opts.search) params = params.set('search', opts.search);

    return this.http.get<PaginatedList<GarmentListItem>>(this.baseUrl, { params });
  }

  getGarmentById(id: string): Observable<GarmentDetail> {
    return this.http.get<GarmentDetail>(`${this.baseUrl}/${id}`);
  }

  getPricingMatrix(): Observable<PricingMatrix> {
    return this.http.get<PricingMatrix>(`${this.baseUrl}/pricing-matrix`);
  }

  createGarment(request: CreateGarmentRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  updateGarment(id: string, request: UpdateGarmentRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  deleteGarment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  setPrice(garmentId: string, serviceId: string, pricingType: PricingType, price: number): Observable<string> {
    return this.http.put<string>(`${this.baseUrl}/${garmentId}/prices/${serviceId}`, { pricingType, price });
  }
}
