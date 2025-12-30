import {
    HttpInterceptorFn,
    HttpErrorResponse,
    HttpRequest,
    HttpHandlerFn
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { catchError, filter, switchMap, take } from 'rxjs/operators';
import { AuthFacadeService } from '../services/auth/auth-facade.service';

// Global state for refresh (shared between requests)
let refreshInProgress = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const auth = inject(AuthFacadeService);

    // 1) Preskoči sve AllowAnonymous endpoint-e
    if (isAnonymousEndpoint(req.url)) {
        return next(req); // šalji request bez Authorization header-a
    }

    // 2) Dodaj Authorization header ako postoji access token
    const accessToken = auth.getAccessToken();
    let authReq = req;

    if (accessToken) {
        authReq = req.clone({
            setHeaders: {
                Authorization: `Bearer ${accessToken}`
            }
        });
    }

    // 3) Handle 401 → refresh → retry
return next(authReq).pipe(
  catchError((err) => {

    // 🔴 AKO JE REGISTER / LOGIN → NE DIRAJ 401
    if (
      err instanceof HttpErrorResponse &&
      err.status === 401 &&
      !isAnonymousEndpoint(req.url)
    ) {
      return handle401Error(authReq, next, auth);
    }

    return throwError(() => err);
  })
);

};

function isAnonymousEndpoint(url: string): boolean {
    const lowerUrl = url.toLowerCase();
    // FORCED: preskači sve AllowAnonymous (login, register, refresh, logout)
    return lowerUrl.includes('/api/auth/login') 
        || lowerUrl.includes('/api/auth/register') 
        || lowerUrl.includes('/api/auth/refresh') 
        || lowerUrl.includes('/api/auth/logout');
}

function handle401Error(
    req: HttpRequest<unknown>,
    next: HttpHandlerFn,
    auth: AuthFacadeService
): Observable<any> {
    const refreshToken = auth.getRefreshToken();

    if (!refreshToken) {
        auth.redirectToLogin();
        return throwError(() => new Error('No refresh token'));
    }

    if (refreshInProgress) {
        return refreshTokenSubject.pipe(
            filter((token) => token !== null),
            take(1),
            switchMap((token) => {
                const cloned = token
                    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
                    : req;
                return next(cloned);
            })
        );
    }

    refreshInProgress = true;
    refreshTokenSubject.next(null);

    return auth.refresh({ refreshToken, fingerprint: null }).pipe(
        switchMap((res) => {
            refreshInProgress = false;
            const newAccessToken = res.accessToken;
            refreshTokenSubject.next(newAccessToken);

            const clonedReq = req.clone({
                setHeaders: { Authorization: `Bearer ${newAccessToken}` }
            });

            return next(clonedReq);
        }),
        catchError((error) => {
            refreshInProgress = false;
            refreshTokenSubject.next(null);
            auth.redirectToLogin();
            return throwError(() => error);
        })
    );
}
