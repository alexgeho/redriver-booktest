import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResponse } from '../models/models';

const TOKEN_KEY = 'bq_token';
const USER_KEY = 'bq_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = `${environment.apiUrl}/auth`;

  // Reactive auth state.
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly _username = signal<string | null>(localStorage.getItem(USER_KEY));

  readonly username = this._username.asReadonly();
  readonly isLoggedIn = computed(() => !!this._token());

  constructor(private http: HttpClient) {}

  get token(): string | null {
    return this._token();
  }

  register(username: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.api}/register`, { username, password })
      .pipe(tap((res) => this.setSession(res)));
  }

  login(username: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.api}/login`, { username, password })
      .pipe(tap((res) => this.setSession(res)));
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._token.set(null);
    this._username.set(null);
  }

  private setSession(res: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(USER_KEY, res.username);
    this._token.set(res.token);
    this._username.set(res.username);
  }
}
