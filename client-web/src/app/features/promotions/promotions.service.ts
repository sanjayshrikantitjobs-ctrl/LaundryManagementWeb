import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActivePromotion, PromotionListItem, SavePromotionRequest } from '../../core/models/promotion.models';
import { PaginatedList, SortDirection } from '../../core/models/order.models';

@Injectable({ providedIn: 'root' })
export class PromotionsService {
  private readonly baseUrl = '/api/v1/promotions';

  constructor(private http: HttpClient) {}

  getActivePromotions(): Observable<ActivePromotion[]> {
    return this.http.get<ActivePromotion[]>(`${this.baseUrl}/active`);
  }

  getPromotions(opts: {
    search?: string;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: SortDirection;
  } = {}): Observable<PaginatedList<PromotionListItem>> {
    let params = new HttpParams()
      .set('pageNumber', opts.pageNumber ?? 1)
      .set('pageSize', opts.pageSize ?? 20);

    if (opts.search) params = params.set('search', opts.search);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDirection) params = params.set('sortDirection', opts.sortDirection);

    return this.http.get<PaginatedList<PromotionListItem>>(this.baseUrl, { params });
  }

  createPromotion(request: SavePromotionRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  updatePromotion(id: string, request: SavePromotionRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  deletePromotion(id: string, reason?: string): Observable<void> {
    let params = new HttpParams();
    if (reason) params = params.set('reason', reason);
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { params });
  }
}
