import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfileResponse } from '../models/user-profile';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminUsersService {
  private readonly http = inject(HttpClient);
   private baseUrl = (environment as { apiUrl?: string })?.apiUrl || 'http://localhost:5133/api';

  getAllUsers(): Observable<UserProfileResponse[]> {
    return this.http.get<UserProfileResponse[]>(`${this.baseUrl}/Profile/all`);
  }
  getAllAdminUsers(): Observable<UserProfileResponse[]> {
    return this.http.get<UserProfileResponse[]>(`${this.baseUrl}/Profile/all/Admin`);
  }

  getUserById(id: string): Observable<UserProfileResponse> {
    return this.http.get<UserProfileResponse>(`${this.baseUrl}/Profile/${id}`);
  }

  updateRole(payload: { id: string; userRole: string }): Observable<UserProfileResponse> {
    return this.http.patch<UserProfileResponse>(`${this.baseUrl}/Profile/role`, payload);
  }

  updateStatus(payload: { id: string; isActive: boolean }): Observable<UserProfileResponse> {
    return this.http.patch<UserProfileResponse>(`${this.baseUrl}/Profile/status`, payload);
  }

  updateManager(payload: { id: string; managerId: string }): Observable<UserProfileResponse> {
    return this.http.patch<UserProfileResponse>(`${this.baseUrl}/Profile/manager`, payload);
  }
}