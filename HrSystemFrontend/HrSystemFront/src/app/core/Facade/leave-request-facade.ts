import { Injectable, inject, signal, computed } from '@angular/core';
import { LeaveRequestService } from '../Services/leave-request-service';
import { LeaveRequest , CreateUpdateLeaveRequest} from '../models/leave-request';

@Injectable({
  providedIn: 'root'
})
export class LeaveRequestFacade {
  private readonly leaveRequestService = inject(LeaveRequestService);

  readonly requests = signal<LeaveRequest[]>([]);
  readonly selectedRequest = signal<LeaveRequest | null>(null);
  readonly approvedRequests = computed(() => 
    this.requests().filter(req => req.status?.toLowerCase() === 'approved')
  );
  

  readonly isLoading = signal<boolean>(false);
  
  canCancelRequest(req: any): boolean {
  return this.leaveRequestService.canBeCancelled(req);
}
  loadMyRequests(): void {
    this.isLoading.set(true);
    this.leaveRequestService.getMyRequests().subscribe({
      next: (data) => {
        this.requests.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading my requests', err);
        this.isLoading.set(false);
      }
    });
  }
  loadRequestById(id: string): void {
 this.isLoading.set(true);

    this.leaveRequestService.getRequestById(id).subscribe({
      next: (data) => {
        this.selectedRequest.set(data);
        this.isLoading.set(false);
        console.log(data);
      },
      error: (err) => {
        this.isLoading.set(false);
        console.error(` Error: Failed to retrieve leave request with ID [${id}].`, err);
      }
    });
  }

  createLeaveRequest(data: CreateUpdateLeaveRequest, onSuccess: () => void): void {
   
    this.leaveRequestService.createRequest(data).subscribe({
      next: (res) => {
        console.log(res);
        this.loadMyRequests();
        onSuccess();
      },
      error: (err) => {
        console.error(' Error: Failed to create leave request.', err);
      }
    });
  }

  updateLeaveRequest(id: string, data: CreateUpdateLeaveRequest, onSuccess: () => void): void {
   
    this.leaveRequestService.updateRequest(id, data).subscribe({
      next: (res) => {
        console.log(res);
        this.loadMyRequests();
        onSuccess();
      },
      error: (err) => {
        console.error(` Error: Failed to update leave request [${id}].`, err);
      }
    });
  }
  cancelLeaveRequest(id: string): void {
  this.leaveRequestService.cancelRequest(id).subscribe({
    next: (res) => {
      console.log(res);
      this.loadMyRequests();
    },
    error: (err) => {
      console.error(`Error: Failed to cancel leave request [${id}].`, err);
    }
  });
}


}