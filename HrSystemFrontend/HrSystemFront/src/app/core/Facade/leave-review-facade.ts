import { inject, Injectable, signal } from '@angular/core';
import {ReviewLeaveResponse,MessageResponse,RejectionRequest} from '../models/review-leave'
import { ACTIVE_PENDING_STRATEGY } from '../Services/strategy/stragegy';

import {LeaveReviewService} from '../Services/leave-review-service'
@Injectable({ providedIn: 'root' })
export class LeaveReviewFacade {
  private readonly service = inject(LeaveReviewService);
  
  private readonly activeStrategy = inject(ACTIVE_PENDING_STRATEGY);

  readonly requests = signal<ReviewLeaveResponse[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  loadPendingRequests(): void {
    this.isLoading.set(true);
    this.clearMessages();

    this.activeStrategy.fetchPending().subscribe({
      next: (data) => {
        this.requests.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        const backendMsg = err?.error?.message || err?.message || 'Failed to load pending requests.';
        this.errorMessage.set(backendMsg);
        this.isLoading.set(false);
      }
    });
  }

  acceptRequest(id: string): void {
    this.clearMessages();
    this.service.acceptRequest(id).subscribe({
      next: (res) => {
        const successMsg = res?.message || 'Request accepted successfully.';
        this.successMessage.set(successMsg);
        this.loadPendingRequests(); 
      },
      error: (err) => {
        const backendMsg = err?.error?.message || err?.message || 'Failed to accept request.';
        this.errorMessage.set(backendMsg);
      }
    });
  }

  rejectRequest(id: string, payload: RejectionRequest): void {
    this.clearMessages();
    this.service.rejectRequest(id, payload).subscribe({
      next: (res) => {
        const successMsg = res?.message || 'Request rejected successfully.';
        this.successMessage.set(successMsg);
        this.loadPendingRequests(); 
      },
      error: (err) => {
        const backendMsg = err?.error?.message || err?.message || 'Failed to reject request.';
        this.errorMessage.set(backendMsg);
      }
    });
  }

  private clearMessages(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }
}