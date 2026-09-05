import { Injectable, inject, signal } from '@angular/core';
import {  PublicHolidayResponse, PublicHolidayRequest } from '../models/public-holiday';
import{PublicHolidayService} from '../Services/public-holiday-service'
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PublicHolidayFacade {
  private readonly holidayService = inject(PublicHolidayService);

  readonly holidays = signal<PublicHolidayResponse[]>([]);
  readonly futureHolidays = signal<PublicHolidayResponse[]>([]);
  readonly selectedHoliday = signal<PublicHolidayResponse | null>(null);
  
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  loadAll(): void {
    this.isLoading.set(true);
    this.clearMessages();
    this.holidayService.getAll().subscribe({
      next: (data) => {
        this.holidays.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to load holidays.');
        this.isLoading.set(false);
      }
    });
  }

  loadAllFuture(): void {
    this.isLoading.set(true);
    this.clearMessages();
    this.holidayService.getAllFuture().subscribe({
      next: (data) => {
        this.futureHolidays.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to load future holidays.');
        this.isLoading.set(false);
      }
    });
  }

  createHoliday(data: PublicHolidayRequest, onSuccess?: () => void): void {
    this.isLoading.set(true);
    this.clearMessages();
    this.holidayService.create(data).subscribe({
      next: (res) => {
        this.successMessage.set('Holiday created successfully.');
        this.isLoading.set(false);
        this.loadAll();
        this.loadAllFuture();
        if (onSuccess) onSuccess();
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to create holiday.');
        this.isLoading.set(false);
      }
    });
  }

  updateHoliday(id: string, data: PublicHolidayRequest, onSuccess?: () => void): void {
    this.isLoading.set(true);
    this.clearMessages();
    this.holidayService.update(id, data).subscribe({
      next: (res) => {
        this.successMessage.set('Holiday updated successfully.');
        this.isLoading.set(false);
        this.loadAll();
        this.loadAllFuture();
        if (onSuccess) onSuccess();
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to update holiday.');
        this.isLoading.set(false);
      }
    });
  }

  deleteHoliday(id: string): void {
    this.isLoading.set(true);
    this.clearMessages();
    this.holidayService.delete(id).subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Holiday deleted successfully.');
        this.isLoading.set(false);
        this.loadAll();
        this.loadAllFuture();
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to delete holiday.');
        this.isLoading.set(false);
      }
    });
  }

  clearMessages(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }
}