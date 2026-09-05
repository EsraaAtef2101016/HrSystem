import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import{PublicHolidayResponse,PublicHolidayRequest ,MessageResponse } from '../../core/models/public-holiday'

import { environment } from '../../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class PublicHolidayService {
  private readonly http = inject(HttpClient);
   private readonly baseUrl = (environment as { apiUrl?: string })?.apiUrl || 'http://localhost:5133/api';

  getAllFuture(): Observable<PublicHolidayResponse[]> {
    return this.http.get<PublicHolidayResponse[]>(`${this.baseUrl}/public-holidays/AllFuture`);
  }

  getAll(): Observable<PublicHolidayResponse[]> {
    return this.http.get<PublicHolidayResponse[]>(`${this.baseUrl}/public-holidays/All`);
  }

  getById(id: string): Observable<PublicHolidayResponse> {
    return this.http.get<PublicHolidayResponse>(`${this.baseUrl}/public-holidays/${id}`);
  }

  create(data: PublicHolidayRequest): Observable<PublicHolidayResponse> {
    return this.http.post<PublicHolidayResponse>(`${this.baseUrl}/public-holidays/create`, data);
  }

  update(id: string, data: PublicHolidayRequest): Observable<PublicHolidayResponse> {
    return this.http.put<PublicHolidayResponse>(`${this.baseUrl}/public-holidays/update/${id}`, data);
  }

  delete(id: string): Observable<MessageResponse> {
    return this.http.delete<MessageResponse>(`${this.baseUrl}/public-holidays/delete/${id}`);
  }
}