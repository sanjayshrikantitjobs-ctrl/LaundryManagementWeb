import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AddOn, CreateAddOnRequest, UpdateAddOnRequest } from '../../core/models/catalog.models';

@Injectable({ providedIn: 'root' })
export class AddOnsService {
  private readonly baseUrl = '/api/v1/addons';

  constructor(private http: HttpClient) {}

  getAddOns(isActive?: boolean): Observable<AddOn[]> {
    let params = new HttpParams();
    if (isActive !== undefined) params = params.set('isActive', isActive);
    return this.http.get<AddOn[]>(this.baseUrl, { params });
  }

  createAddOn(request: CreateAddOnRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  updateAddOn(id: string, request: UpdateAddOnRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  deleteAddOn(id: string, reason?: string): Observable<void> {
    let params = new HttpParams();
    if (reason) params = params.set('reason', reason);
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { params });
  }
}
