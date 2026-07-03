import { HttpInterceptorFn } from '@angular/common/http';

// Her HTTP isteğine JWT token ekler (admin API çağrıları için gerekli).
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');

  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(req);
};
