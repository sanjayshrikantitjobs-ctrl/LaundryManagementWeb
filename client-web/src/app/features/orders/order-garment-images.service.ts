import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AddOrderGarmentImageRequest, OrderGarmentImage } from '../../core/models/order-garment-image.models';

@Injectable({ providedIn: 'root' })
export class OrderGarmentImagesService {
  constructor(private http: HttpClient) {}

  getImages(orderId: string): Observable<OrderGarmentImage[]> {
    return this.http.get<OrderGarmentImage[]>(`/api/v1/orders/${orderId}/images`);
  }

  addImage(orderId: string, request: AddOrderGarmentImageRequest): Observable<string> {
    return this.http.post<string>(`/api/v1/orders/${orderId}/images`, request);
  }

  deleteImage(orderId: string, imageId: string): Observable<void> {
    return this.http.delete<void>(`/api/v1/orders/${orderId}/images/${imageId}`);
  }
}
