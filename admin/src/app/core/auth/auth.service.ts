import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map, tap } from 'rxjs';

import { environment } from '../../../environments/environment';

interface LoginResponse {
  token: string;
}

interface JwtPayload {
  sub?: string;
  email?: string;
  exp?: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private token: string | null = null;
  private readonly TOKEN_KEY = 'sichi_jwt';

  login(email: string, password: string): Observable<void> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, {
        email,
        password,
      })
      .pipe(
        tap((response) => {
          this.token = response.token;
        }),
        map(() => void 0)
      );
  }

  logout(): void {
    this.token = null;
    void this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.token;
  }

  isAuthenticated(): boolean {
    if (this.token === null) {
      return false;
    }

    const payload = this.decodePayload();
    if (!payload?.exp) {
      this.token = null;
      return false;
    }

    if (payload.exp <= Date.now() / 1000) {
      this.token = null;
      return false;
    }

    return true;
  }

  getUserId(): number | null {
    if (!this.token) {
      return null;
    }

    const payload = this.decodePayload();
    if (!payload?.sub) {
      return null;
    }

    const id = parseInt(payload.sub, 10);
    return Number.isNaN(id) ? null : id;
  }

  getUserEmail(): string | null {
    if (!this.token) {
      return null;
    }

    const payload = this.decodePayload();
    return payload?.email ?? null;
  }

  private decodePayload(): JwtPayload | null {
    if (!this.token) {
      return null;
    }

    const parts = this.token.split('.');
    if (parts.length < 2) {
      return null;
    }

    try {
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = atob(base64);
      return JSON.parse(json) as JwtPayload;
    } catch {
      return null;
    }
  }
}
