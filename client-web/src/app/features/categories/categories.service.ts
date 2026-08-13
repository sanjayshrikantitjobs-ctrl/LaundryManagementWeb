import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateServiceCategoryRequest,
  ServiceCategory,
  UpdateServiceCategoryRequest
} from '../../core/models/catalog.models';

@Injectable({ providedIn: 'root' })
export class CategoriesService {
  private readonly baseUrl = '/api/v1/servicecategories';

  constructor(private http: HttpClient) {}

  getCategories(): Observable<ServiceCategory[]> {
    return this.http.get<ServiceCategory[]>(this.baseUrl);
  }

  createCategory(request: CreateServiceCategoryRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  updateCategory(id: string, request: UpdateServiceCategoryRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  deleteCategory(id: string, reason?: string): Observable<void> {
    let params = new HttpParams();
    if (reason) params = params.set('reason', reason);
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { params });
  }
}
