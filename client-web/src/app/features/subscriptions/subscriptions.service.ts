import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AssignCustomerSubscriptionRequest,
  CreateSubscriptionPlanRequest,
  CustomerSubscriptionDetail,
  CustomerSubscriptionListItem,
  SubscriptionPlan,
  UpdateCustomerSubscriptionRequest,
  UpdateSubscriptionPlanRequest
} from '../../core/models/subscription.models';
import { PaginatedList } from '../../core/models/order.models';

@Injectable({ providedIn: 'root' })
export class SubscriptionsService {
  private readonly plansUrl = '/api/v1/subscriptionplans';
  private readonly customerSubscriptionsUrl = '/api/v1/customersubscriptions';

  constructor(private http: HttpClient) {}

  getPlans(): Observable<SubscriptionPlan[]> {
    return this.http.get<SubscriptionPlan[]>(this.plansUrl);
  }

  createPlan(request: CreateSubscriptionPlanRequest): Observable<string> {
    return this.http.post<string>(this.plansUrl, request);
  }

  updatePlan(id: string, request: UpdateSubscriptionPlanRequest): Observable<void> {
    return this.http.put<void>(`${this.plansUrl}/${id}`, request);
  }

  deletePlan(id: string, reason?: string): Observable<void> {
    let params = new HttpParams();
    if (reason) params = params.set('reason', reason);
    return this.http.delete<void>(`${this.plansUrl}/${id}`, { params });
  }

  getCustomerSubscriptions(opts: {
    search?: string;
    customerId?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Observable<PaginatedList<CustomerSubscriptionListItem>> {
    let params = new HttpParams()
      .set('pageNumber', opts.pageNumber ?? 1)
      .set('pageSize', opts.pageSize ?? 20);

    if (opts.search) params = params.set('search', opts.search);
    if (opts.customerId) params = params.set('customerId', opts.customerId);

    return this.http.get<PaginatedList<CustomerSubscriptionListItem>>(this.customerSubscriptionsUrl, { params });
  }

  getCustomerSubscriptionById(id: string): Observable<CustomerSubscriptionDetail> {
    return this.http.get<CustomerSubscriptionDetail>(`${this.customerSubscriptionsUrl}/${id}`);
  }

  assignSubscription(request: AssignCustomerSubscriptionRequest): Observable<string> {
    return this.http.post<string>(this.customerSubscriptionsUrl, request);
  }

  updateSubscription(id: string, request: UpdateCustomerSubscriptionRequest): Observable<void> {
    return this.http.put<void>(`${this.customerSubscriptionsUrl}/${id}`, request);
  }
}
