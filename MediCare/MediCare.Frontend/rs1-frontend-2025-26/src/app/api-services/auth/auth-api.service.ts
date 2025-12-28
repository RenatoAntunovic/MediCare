import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LoginCommand,
  LoginCommandDto,
  RefreshTokenCommand,
  RefreshTokenCommandDto,
  LogoutCommand
} from './auth-api.model';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/Auth`;
  private http = inject(HttpClient);
  currentUserId: number | null = null;

  /**
   * POST /Auth/login
   * Authenticate user and get access/refresh tokens.
   */
  login(payload: LoginCommand): Observable<LoginCommandDto> {
    return this.http.post<LoginCommandDto>(`${this.baseUrl}/login`, payload);
  }

  /**
   * POST /Auth/refresh
   * Refresh access token using refresh token.
   */
  refresh(payload: RefreshTokenCommand): Observable<RefreshTokenCommandDto> {
    return this.http.post<RefreshTokenCommandDto>(`${this.baseUrl}/refresh`, payload);
  }


  setCurrentUserId(id: number) {
    this.currentUserId = id;
    localStorage.setItem('currentUserId', id.toString()); // opcionalno za reload
    console.log('User logged in, currentUserId set to:', id);
  }

  // dohvat userId za checkout
  getCurrentUserId(): number {
    if (this.currentUserId === null) {
      // pokušaj dohvatiti iz localStorage
      const stored = localStorage.getItem('currentUserId');
      if (stored) {
        this.currentUserId = parseInt(stored, 10);
      } else {
        throw new Error('No user logged in');
      }
    }
    return this.currentUserId;
  }


  /**
   * POST /Auth/logout
   * Invalidate refresh token and logout user.
   */
  logout(payload: LogoutCommand): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, payload);
  }
}
