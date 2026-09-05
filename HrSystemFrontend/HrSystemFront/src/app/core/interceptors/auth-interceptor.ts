import { HttpInterceptorFn } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthFacade } from '../Facade/auth-facade';
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authFacade = inject(AuthFacade);
  const router = inject(Router);   // <-- just inject directly

  const urlsToExclude = ['/api/auth/login', '/api/auth/register'];
  const isExcluded = urlsToExclude.some(url => req.url.includes(url));

  if (typeof window === 'undefined' || isExcluded) {
    return next(req);
  }

  const token = localStorage.getItem('token');
  let apiReq = req;

  if (token) {
    apiReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(apiReq).pipe(
    catchError((error) => {
      if (error.status === 401) {
        authFacade.logout();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};