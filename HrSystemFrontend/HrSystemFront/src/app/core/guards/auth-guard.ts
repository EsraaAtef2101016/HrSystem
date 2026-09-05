import { CanActivateFn,Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthFacade } from '../../core/Facade/auth-facade';
export const authGuard: CanActivateFn = (route, state) => {
  const router= inject(Router);

  const authFacade = inject(AuthFacade);
  const token = authFacade.getAuthToken()
  if (token){
     return true;
  }else{
    router.navigate(['/login']);
    return false;
  }
};
