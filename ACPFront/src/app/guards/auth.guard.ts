import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';

export const authGuard: CanActivateFn = (route, state): boolean => {
  const jwt = new JwtHelperService();
  const token = localStorage.getItem('token');

  if (!token || jwt.isTokenExpired(token)) {
    const router = inject(Router);
    router.navigateByUrl('/auth/sign-in');
  }

  return true;
};
