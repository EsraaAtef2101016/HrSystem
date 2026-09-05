import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LeaveRequest, CreateUpdateLeaveRequest } from '../models/leave-request';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LeaveRequestService {
  private readonly http = inject(HttpClient);
     private apiUrl = (environment as { apiUrl?: string })?.apiUrl || 'http://localhost:5133/api';
    

  getMyRequests(): Observable<LeaveRequest[]> {
    return this.http.get<LeaveRequest[]>(`${this.apiUrl}/LeaveRequest/my-requests`);
  }
  getRequestById(id: string): Observable<LeaveRequest> {
    return this.http.get<LeaveRequest>(`${this.apiUrl}/${id}`);
  }

  createRequest(data: CreateUpdateLeaveRequest): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(`${this.apiUrl}/LeaveRequest/create`, data);
  }

  updateRequest(id: string, data: CreateUpdateLeaveRequest): Observable<LeaveRequest> {
    return this.http.put<LeaveRequest>(`${this.apiUrl}/LeaveRequest/update/${id}`, data);
  }

  cancelRequest(id: string): Observable<LeaveRequest> {
    return this.http.patch<LeaveRequest>(`${this.apiUrl}/LeaveRequest/cancel/${id}`,{});
}
canBeCancelled(request: LeaveRequest): boolean {
  if (!request || request.status?.toLowerCase() === 'cancelled' ||  request.status?.toLowerCase() === 'rejected') {
    return true;
  }
  if(request.status?.toLowerCase() === 'pending')
    return false;
  if (request.status?.toLowerCase() === 'approved'){
  if (!request.startDate) return false;

  const startDate = new Date(request.startDate);
  const today = new Date();
  
  today.setHours(0, 0, 0, 0);
  startDate.setHours(0, 0, 0, 0);

  return startDate < today;}
  return true;
}
}