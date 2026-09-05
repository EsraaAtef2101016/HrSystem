// 2. Updated Employee Service (employee.service.ts)
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EmployeeParticipationStatus, LeaveBalance } from '../models/dashboard';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly http = inject(HttpClient);
   private apiUrl = (environment as { apiUrl?: string })?.apiUrl || 'http://localhost:5133/api';
  
  getParticipationStatus(): Observable<EmployeeParticipationStatus> {
    return this.http.get<EmployeeParticipationStatus>(`${this.apiUrl}/EmployeeParticipation/status`);
  }

  getCurrentLeaveBalances(): Observable<LeaveBalance[]> {
    return this.http.get<LeaveBalance[]>(`${this.apiUrl}/LeaveRequest/balances/current`);
  }
}