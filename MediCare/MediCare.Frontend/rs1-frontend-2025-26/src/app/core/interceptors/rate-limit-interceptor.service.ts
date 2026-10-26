import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToasterService } from '../services/toaster.service';

// Several requests can fail at the same moment – show only one message
let lastToastAt = 0;

/**
 * Shows a friendly message when the API returns 429 Too Many Requests (rate limiting).
 * Uses the Retry-After header (seconds) to tell the user how long to wait.
 */
export const rateLimitInterceptor: HttpInterceptorFn = (req, next) => {
  const toaster = inject(ToasterService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 429) {
        const now = Date.now();
        if (now - lastToastAt > 3000) {
          lastToastAt = now;
          const retryAfter = Number(error.headers.get('Retry-After'));
          toaster.error(
            retryAfter > 0
              ? `Previše zahtjeva. Pokušajte ponovo za ${retryAfter} s.`
              : 'Previše zahtjeva. Sačekajte malo pa pokušajte ponovo.'
          );
        }
      }
      return throwError(() => error);
    })
  );
};