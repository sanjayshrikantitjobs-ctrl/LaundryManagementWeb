import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateUserRequest, UpdateUserRequest, UserRole, UserSummary } from '../../core/models/user.models';
import { PaginatedList, SortDirection } from '../../core/models/order.models';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly baseUrl = '/api/v1/users';

  constructor(private http: HttpClient) {}

  getUsers(opts: {
    search?: string;
    role?: UserRole;
    isActive?: boolean;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: SortDirection;
  } = {}): Observable<PaginatedList<UserSummary>> {
    let params = new HttpParams()
      .set('pageNumber', opts.pageNumber ?? 1)
      .set('pageSize', opts.pageSize ?? 20);

    if (opts.search) params = params.set('search', opts.search);
    if (opts.role !== undefined) params = params.set('role', opts.role);
    if (opts.isActive !== undefined) params = params.set('isActive', opts.isActive);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDirection) params = params.set('sortDirection', opts.sortDirection);

    return this.http.get<PaginatedList<UserSummary>>(this.baseUrl, { params });
  }

  getUserById(id: string): Observable<UserSummary> {
    return this.http.get<UserSummary>(`${this.baseUrl}/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  setActive(id: string, isActive: boolean): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/active`, { isActive });
  }

  assignRole(id: string, role: UserRole): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/role`, { role });
  }
}
