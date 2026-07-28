import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse } from '../models/order.models';

const TOKEN_KEY = 'laundry_mgmt_access_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _currentUser = signal<LoginResponse | null>(this.restoreSession());
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => !!this._currentUser());

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/v1/auth/login', request).pipe(
      tap((response) => {
        this._currentUser.set(response);
        sessionStorage.setItem(TOKEN_KEY, JSON.stringify(response));
      })
    );
  }

  logout(): void {
    this._currentUser.set(null);
    sessionStorage.removeItem(TOKEN_KEY);
  }

  getAccessToken(): string | null {
    return this._currentUser()?.accessToken ?? null;
  }

  private restoreSession(): LoginResponse | null {
    const raw = sessionStorage.getItem(TOKEN_KEY);
    return raw ? (JSON.parse(raw) as LoginResponse) : null;
  }
}
