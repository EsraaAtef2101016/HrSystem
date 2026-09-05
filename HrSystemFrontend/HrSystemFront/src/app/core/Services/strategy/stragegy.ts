import { inject, Injectable, InjectionToken, PLATFORM_ID } from '@angular/core';
import { Observable } from 'rxjs';
import {  ReviewLeaveResponse } from '../../models/review-leave';
import {LeaveReviewService} from '../../Services/leave-review-service';
import { isPlatformBrowser } from '@angular/common';
export interface IPendingLeaveStrategy {
  fetchPending(): Observable<ReviewLeaveResponse[]>;
}

@Injectable({ providedIn: 'root' })
export class ManagerPendingStrategy implements IPendingLeaveStrategy {
  private readonly service = inject(LeaveReviewService);
  
  fetchPending(): Observable<ReviewLeaveResponse[]> {
    return this.service.getManagerPending();
  }
}

@Injectable({ providedIn: 'root' })
export class AdminPendingStrategy implements IPendingLeaveStrategy {
  private readonly service = inject(LeaveReviewService);
  
  fetchPending(): Observable<ReviewLeaveResponse[]> {
    return this.service.getAdminPending();
  }
}

export const ACTIVE_PENDING_STRATEGY = new InjectionToken<IPendingLeaveStrategy>('ActivePendingStrategy', {
  providedIn: 'root',
  factory: () => {
    const platformId = inject(PLATFORM_ID);
    let role = 'manager';

    if (isPlatformBrowser(platformId)) {
      role = (localStorage.getItem('role') as string)?.toLowerCase() || 'manager';
    }

    if (role === 'admin') {
      return inject(AdminPendingStrategy);
    }
    return inject(ManagerPendingStrategy);
  }
});