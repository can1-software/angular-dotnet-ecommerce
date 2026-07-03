import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/auth.models';

// Sadece Admin rolündeki kullanıcılar admin sayfalarına girebilir.
export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn() && auth.user()?.role === UserRole.Admin) {
    return true;
  }

  router.navigate(['/']);
  return false;
};
