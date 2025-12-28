import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import{ToastrService} from 'ngx-toastr';

@Injectable()
export class RateLimitInterceptor implements HttpInterceptor {

  constructor(private toastr: ToastrService) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {

        if (error.status === 429) {
          this.toastr.warning(
            'Previše zahtjeva. Sačekaj malo pa pokušaj ponovo.',
            'Rate limit'
          );
        }

        return throwError(() => error);
      })
    );
  }
}
