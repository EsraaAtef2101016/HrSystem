import { Injectable, inject, signal } from '@angular/core';
import { ParticipationStatus } from '../models/participation';
import { EmployeeParticipationService } from '../Services/employee-participation-service';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EmployeeParticipationFacade {
  private readonly participationService = inject(EmployeeParticipationService);

  readonly status = signal<ParticipationStatus | null>(null);
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  loadStatus(): void {
    this.isLoading.set(true);
    this.participationService.getStatus().subscribe({
      next: (data) => {
        this.status.set(data);
        this.isLoading.set(false);
        console.log("get Status", data);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to load status.');
        this.isLoading.set(false);
        console.error(err);
      }
    });
  }

  optIn(onSuccess?: () => void): void {
    this.clearMessages();
    this.participationService.optIn().subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Opted in successfully.');
        this.loadStatus();
        if (onSuccess) onSuccess();
        console.log("optIn :", res);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to opt in.');
        console.error(err);
      }
    });
  }

  optOut(onSuccess?: () => void): void {
    this.clearMessages();
    this.participationService.optOut().subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Opted out successfully.');
        this.loadStatus();
        if (onSuccess) onSuccess();
        console.log("optOut :", res);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to opt out.');
        console.error(err);
      }
    });
  }

  forceParticipation(employeeId: string, forceOptIn: boolean, reason: string): void {
    this.isLoading.set(true);
    this.clearMessages();

    this.participationService.forceParticipation(employeeId, { forceOptIn, reason }).subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Employee participation forced successfully.');
        this.isLoading.set(false);
        console.log("force Participation :", res);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to force employee participation.');
        this.isLoading.set(false);
        console.error(err);
      }
    });
  }

  updatePolicy(isSelfOptOutAllowed: boolean, cooldownDays: number): void {
    this.isLoading.set(true);
    this.clearMessages();

    this.participationService.updatePolicy({ isSelfOptOutAllowed, cooldownDays }).subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Participation policy updated successfully.');
        this.isLoading.set(false);
        console.log("update Policy :", res);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to update participation policy.');
        this.isLoading.set(false);
        console.error(err);
      }
    });
  }

  readonly employeeStatuses = signal<{ [key: string]: ParticipationStatus }>({});

  loadEmployeeStatus(employeeId: string): void {
    if (this.employeeStatuses()[employeeId]) return;

    this.participationService.getEmployeeStatus(employeeId).pipe(
      tap((res: ParticipationStatus) => console.log(`getEmployeeStatus for ${employeeId} Response:`, res))
    ).subscribe({
      next: (data: ParticipationStatus) => {
        this.employeeStatuses.update(current => ({ ...current, [employeeId]: data }));
      },
      error: (err) => console.error(`getEmployeeStatus Error for ${employeeId}:`, err)
    });
  }

  clearMessages(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }
}