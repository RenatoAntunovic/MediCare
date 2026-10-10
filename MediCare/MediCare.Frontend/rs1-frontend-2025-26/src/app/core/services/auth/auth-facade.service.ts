// src/app/core/services/auth/auth-facade.service.ts
import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of, tap, catchError, map, finalize } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

import { AuthApiService } from '../../../api-services/auth/auth-api.service';
import {
  LoginCommand,
  LoginCommandDto,
  LogoutCommand,
  RefreshTokenCommand,
  RefreshTokenCommandDto,
} from '../../../api-services/auth/auth-api.model';

import { AuthStorageService } from './auth-storage.service';
import { CurrentUserDto } from './current-user.dto';
import { JwtPayloadDto } from './jwt-payload.dto';

/**
 * Main auth service (façade).
 * - talks to AuthApiService (HTTP)
 * - talks to AuthStorageService (localStorage)
 * - decodes the JWT and holds the current user as a signal
 *
 * Used in:
 * - the interceptor (getAccessToken, refresh)
 * - guards (isAuthenticated, isAdmin)
 * - components (login, logout, navbar)
 */
@Injectable({ providedIn: 'root' })
export class AuthFacadeService {
  private api = inject(AuthApiService);
  private storage = inject(AuthStorageService);
  private router = inject(Router);

  // === REACTIVE STATE: current user ===

  private _currentUser = signal<CurrentUserDto | null>(null);

  /** Read-only signal for the UI – read as auth.currentUser() */
  currentUser = this._currentUser.asReadonly();

  /** Computed signals based on the current user */
  isAuthenticated = computed(() => !!this._currentUser());
  isAdmin = computed(() => this._currentUser()?.isAdmin ?? false);
  isManager = computed(() => this._currentUser()?.isManager ?? false);
  isEmployee = computed(() => this._currentUser()?.isEmployee ?? false);

  constructor() {
    // Try to restore state from an existing access token
    this.initializeFromToken();
  }

  // =========================================================
  // PUBLIC API
  // =========================================================

  /**
   * Logs the user in (email + password).
   * Saves tokens to storage, decodes the JWT and sets the current user state.
   */
  login(payload: LoginCommand): Observable<CurrentUserDto> {
    return this.api.login(payload).pipe(
      tap((response: LoginCommandDto) => {
        this.storage.saveLogin(response);
      }),
      map((response: LoginCommandDto) => {
        const token = response.accessToken;
        const payloadDecoded = jwtDecode<any>(token);

        const roleName = payloadDecoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? '';
        const user: CurrentUserDto = {
          userId: Number(payloadDecoded.sub),
          email: payloadDecoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
          isAdmin: roleName === 'Admin',
          isManager: roleName === 'Manager',
          isEmployee: roleName === 'User',
          tokenVersion: Number(payloadDecoded.ver),
        };

        this._currentUser.set(user);

        return user; // returns the user to the caller
      })
    );
  }

  /**
   * Logs the user out:
   * - revokes the refresh token on the server (errors are ignored)
   * - clears local state and tokens AFTER the request finishes,
   *   so the interceptor can still attach the access token
   */
  logout(): Observable<void> {
    const refreshToken = this.storage.getRefreshToken();

    // No refresh token → nothing to revoke, just clear locally
    if (!refreshToken) {
      this.clearUserState();
      return of(void 0);
    }

    const payload: LogoutCommand = { refreshToken };

    return this.api.logout(payload).pipe(
      catchError(() => of(void 0)),         // ignore server errors
      finalize(() => this.clearUserState()) // always clear locally at the end
    );
  }

  /**
   * Refreshes the access token using the refresh token.
   * Called by the interceptor when it receives a 401.
   */
  refresh(payload: RefreshTokenCommand): Observable<RefreshTokenCommandDto> {
    return this.api.refresh(payload).pipe(
      tap((response: RefreshTokenCommandDto) => {
        this.storage.saveRefresh(response);          // save the new tokens
        this.decodeAndSetUser(response.accessToken); // update the current user
      })
    );
  }

  /**
   * Utility for guards/interceptors – clears auth state and navigates to login.
   */
  redirectToLogin(): void {
    this.clearUserState();
    this.router.navigate(['/auth/login']);
  }

  // =========================================================
  // GETTERS FOR THE INTERCEPTOR
  // =========================================================

  /**
   * Access token for the Authorization header.
   */
  getAccessToken(): string | null {
    return this.storage.getAccessToken();
  }

  /**
   * Refresh token for the refresh call.
   */
  getRefreshToken(): string | null {
    return this.storage.getRefreshToken();
  }

  // =========================================================
  // PRIVATE HELPERS
  // =========================================================

  /**
   * On app start (constructor) – try to restore state from an existing token.
   */
  private initializeFromToken(): void {
    const token = this.storage.getAccessToken();
    if (token) {
      this.decodeAndSetUser(token);
    }
  }

  /**
   * Decodes the JWT and sets the current user state.
   */
  private decodeAndSetUser(token: string): void {
    try {
      const payload = jwtDecode<any>(token);

      const roleName = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? '';

      const user: CurrentUserDto = {
        userId: Number(payload.sub),
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
        isAdmin: roleName === 'Admin',
        isManager: roleName === 'Manager',
        isEmployee: roleName === 'User',
        tokenVersion: Number(payload.ver),
      };

      this._currentUser.set(user);
    } catch (error) {
      console.error('Failed to decode JWT token:', error);
      this._currentUser.set(null);
    }
  }

  /**
   * Clears the user state and all tokens from storage.
   */
  private clearUserState(): void {
    this._currentUser.set(null);
    this.storage.clear();
  }
}