import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {ReviewLeaveResponse,MessageResponse,RejectionRequest} from '../models/review-leave'

@Injectable({
  providedIn: 'root'
})
export class LeaveReviewService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = (environment as { apiUrl?: string })?.apiUrl || 'http://localhost:5133/api';

  getManagerPending(): Observable<ReviewLeaveResponse[]> {
    return this.http.get<ReviewLeaveResponse[]>(`${this.baseUrl}/LeaveReview/manager/pending`);
  }

  getAdminPending(): Observable<ReviewLeaveResponse[]> {
    return this.http.get<ReviewLeaveResponse[]>(`${this.baseUrl}/LeaveReview/admin/pending`);
  }

  acceptRequest(id: string): Observable<MessageResponse> {
    return this.http.patch<MessageResponse>(`${this.baseUrl}/LeaveReview/${id}/accept`, {});
  }

  rejectRequest(id: string, payload: RejectionRequest): Observable<MessageResponse> {
    return this.http.patch<MessageResponse>(`${this.baseUrl}/LeaveReview/${id}/reject`, payload);
  }
}