// src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, ActivatedRouteSnapshot, Router } from '@angular/router';
import { CurrentUserService } from '../services/auth/current-user.service';

export const myAuthGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const currentUser = inject(CurrentUserService);
  const router = inject(Router);

  // Routes declare their rules with myAuthData(...), which stores them under data['auth'].
  // (Reading data['requireAuth'] directly always gave undefined, so the guard let everybody in.)
  const rules = (route.data['auth'] ?? route.data) as MyAuthRouteData;
  const requireAuth = rules.requireAuth === true;
  const requireAdmin = rules.requireAdmin === true;
  const requireManager = rules.requireManager === true;
  const requireEmployee = rules.requireEmployee === true;

  const isAuth = currentUser.isAuthenticated();

  // 1) ako ruta traži auth, a user nije logiran → login
  if (requireAuth && !isAuth) {
    router.navigate(['/auth/login']);
    return false;
  }

  // Ako ne traži auth → pusti (javne rute)
  if (!requireAuth) {
    return true;
  }

  // 2) role check – admin > manager > employee
  const user = currentUser.snapshot;
  if (!user) {
    router.navigate(['/auth/login']);
    return false;
  }

  if (requireAdmin && !user.isAdmin) {
    router.navigate([currentUser.getDefaultRoute()]);
    return false;
  }

  if (requireManager && !user.isManager) {
    router.navigate([currentUser.getDefaultRoute()]);
    return false;
  }

  if (requireEmployee && !user.isEmployee) {
    router.navigate([currentUser.getDefaultRoute()]);
    return false;
  }

  return true;
};

export interface MyAuthRouteData {
  requireAuth?: boolean;
  requireAdmin?: boolean;
  requireManager?: boolean;
  requireEmployee?: boolean;
}

export function myAuthData(data: MyAuthRouteData): { auth: MyAuthRouteData } {
  return { auth: data };
}
