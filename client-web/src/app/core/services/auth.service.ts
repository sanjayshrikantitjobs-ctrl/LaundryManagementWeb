import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import {
  ForgotPasswordRequest,
  ForgotPasswordResponse,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
  ResetPasswordRequest,
  VerifyOtpRequest
} from '../models/order.models';

const TOKEN_KEY = 'laundry_mgmt_access_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _currentUser = signal<LoginResponse | null>(this.restoreSession());
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => !!this._currentUser());

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/v1/auth/login', request).pipe(
      tap((response) => this.applySession(response))
    );
  }

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>('/api/v1/auth/register', request);
  }

  verifyOtp(request: VerifyOtpRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/v1/auth/verify-otp', request).pipe(
      tap((response) => this.applySession(response))
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<ForgotPasswordResponse> {
    return this.http.post<ForgotPasswordResponse>('/api/v1/auth/forgot-password', request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>('/api/v1/auth/reset-password', request);
  }

  logout(): void {
    this._currentUser.set(null);
    sessionStorage.removeItem(TOKEN_KEY);
  }

  getAccessToken(): string | null {
    return this._currentUser()?.accessToken ?? null;
  }

  private applySession(response: LoginResponse): void {
    this._currentUser.set(response);
    sessionStorage.setItem(TOKEN_KEY, JSON.stringify(response));
  }

  private restoreSession(): LoginResponse | null {
    const raw = sessionStorage.getItem(TOKEN_KEY);
    return raw ? (JSON.parse(raw) as LoginResponse) : null;
  }
}
