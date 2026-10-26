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

// Global refresh state (shared between all requests)
let refreshInProgress = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const auth = inject(AuthFacadeService);

    // 1) Skip anonymous endpoints
    if (isAnonymousEndpoint(req.url)) {
        return next(req); // send request without the Authorization header
    }

    // 2) Attach the Authorization header if an access token exists
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
            // Don't try to refresh on anonymous endpoints (login, register, refresh)
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
    // Login, register and refresh are sent without a token.
    // Logout is NOT here: the backend action has [Authorize], so it needs the Bearer token.
    return lowerUrl.includes('/api/auth/login')
        || lowerUrl.includes('/api/auth/register')
        || lowerUrl.includes('/api/auth/refresh');
}

function handle401Error(
    req: HttpRequest<unknown>,
    next: HttpHandlerFn,
    auth: AuthFacadeService
): Observable<any> {
    const refreshToken = auth.getRefreshToken();

    // No refresh token → the session can't be renewed, go to login
    if (!refreshToken) {
        auth.redirectToLogin();
        return throwError(() => new Error('No refresh token'));
    }

    // A refresh is already running → wait for the new token, then retry
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

    // Start a new refresh
    refreshInProgress = true;
    refreshTokenSubject.next(null);

    return auth.refresh({ refreshToken, fingerprint: null }).pipe(
        switchMap((res) => {
            refreshInProgress = false;
            const newAccessToken = res.accessToken;
            refreshTokenSubject.next(newAccessToken);

            // Retry the original request with the new token
            const clonedReq = req.clone({
                setHeaders: { Authorization: `Bearer ${newAccessToken}` }
            });

            return next(clonedReq);
        }),
        catchError((error) => {
            // Refresh failed → clear state and send the user to login
            refreshInProgress = false;
            refreshTokenSubject.next(null);
            auth.redirectToLogin();
            return throwError(() => error);
        })
    );
}