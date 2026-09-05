import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import{ LeavePolicyResponse,CreateLeavePolicyRequest,UpdateLeavePolicyRequest,UpdatePolicyStatusRequest} from '../models/leave-policy'

import { environment } from '../../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class LeavePolicyService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = (environment as { apiUrl?: string })?.apiUrl || 'http://localhost:5133/api/LeavePolicy';
    
  getAllPolicies(): Observable<LeavePolicyResponse[]> {
    return this.http.get<LeavePolicyResponse[]>(`${this.baseUrl}/all`);
  }

  getPolicyById(id: string): Observable<LeavePolicyResponse> {
    return this.http.get<LeavePolicyResponse[]>(`${this.baseUrl}/${id}`) as unknown as Observable<LeavePolicyResponse>;
  }

  createPolicy(payload: CreateLeavePolicyRequest): Observable<LeavePolicyResponse> {
    return this.http.post<LeavePolicyResponse>(`${this.baseUrl}/create`, payload);
  }

  updatePolicy(payload: UpdateLeavePolicyRequest): Observable<LeavePolicyResponse> {
    return this.http.put<LeavePolicyResponse>(`${this.baseUrl}/update`, payload);
  }

  updatePolicyStatus(payload: UpdatePolicyStatusRequest): Observable<LeavePolicyResponse> {
    return this.http.patch<LeavePolicyResponse>(`${this.baseUrl}/status`, payload);
  }
}