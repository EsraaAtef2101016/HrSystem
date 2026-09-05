// 1. Employee Participation Service (employee-participation.service.ts)
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {ParticipationStatus,MessageResponse} from '../models/participation'

@Injectable({
  providedIn: 'root'
})
export class EmployeeParticipationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5133/api/EmployeeParticipation';

  getStatus(): Observable<ParticipationStatus> {
    return this.http.get<ParticipationStatus>(`${this.baseUrl}/status`);
  }

  getEmployeeStatus(employeeId: string): Observable<ParticipationStatus> {
  return this.http.get<ParticipationStatus>(`${this.baseUrl}/status/${employeeId}`);
}

  optIn(): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.baseUrl}/opt-in`, {});
  }

  optOut(): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.baseUrl}/opt-out`, {});
  }

  forceParticipation(employeeId: string, payload: { forceOptIn: boolean; reason: string }): Observable<MessageResponse> {
    return this.http.patch<MessageResponse>(`${this.baseUrl}/admin/employees/${employeeId}/force-participation`, payload);
  }

  updatePolicy(payload: { isSelfOptOutAllowed: boolean; cooldownDays: number }): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.baseUrl}/admin/policy`, payload);
  }
}