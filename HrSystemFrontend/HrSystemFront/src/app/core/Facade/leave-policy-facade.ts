import { inject, Injectable, signal, computed } from '@angular/core';
import { LeavePolicyResponse, CreateLeavePolicyRequest, UpdateLeavePolicyRequest } from '../models/leave-policy';
import { LeavePolicyService } from '../Services/leave-policy-service';

@Injectable({ providedIn: 'root' })
export class LeavePolicyFacade {
  private readonly service = inject(LeavePolicyService);

  readonly policies = signal<LeavePolicyResponse[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  
  readonly showForm = signal<boolean>(false);
  readonly isEditing = signal<boolean>(false);
  readonly currentEditingId = signal<string | null>(null);

  readonly filterText = signal<string>('');
  readonly statusFilter = signal<'ALL' | 'ENABLED' | 'DISABLED'>('ALL');

  
  readonly totalCount = computed(() => this.policies().length);
  readonly enabledCount = computed(() => this.policies().filter(p => p.isEnabled).length);
  readonly disabledCount = computed(() => this.policies().filter(p => !p.isEnabled).length);

  readonly filteredPolicies = computed(() => {
    const text = this.filterText().toLowerCase().trim();
    const status = this.statusFilter();
    const allPolicies = this.policies();

    return allPolicies.filter(p => {
      const matchesText = !text || 
        String(p.leaveType).toLowerCase().includes(text) ||
        String(p.annualAllowance).includes(text);

      const matchesStatus = 
        status === 'ALL' || 
        (status === 'ENABLED' && p.isEnabled) || 
        (status === 'DISABLED' && !p.isEnabled);

      return matchesText && matchesStatus;
    });
  });

  setFilterText(text: string): void {
    this.filterText.set(text);
  }

  setStatusFilter(status: 'ALL' | 'ENABLED' | 'DISABLED'): void {
    this.statusFilter.set(status);
  }

  loadAllPolicies(): void {
    this.isLoading.set(true);
    this.clearMessages();

    this.service.getAllPolicies().subscribe({
      next: (data) => {
        this.policies.set(data);
        this.isLoading.set(false);
        console.log("get All Policies",data)
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to load leave policies.');
        this.isLoading.set(false);
        console.error(err)
      }
    });
  }

  openCreateForm(): void {
    this.isEditing.set(false);
    this.currentEditingId.set(null);
    this.showForm.set(true);
    this.clearMessages();
  }

  openEditForm(policy: LeavePolicyResponse): void {
    this.isEditing.set(true);
    this.currentEditingId.set(policy.id);
    this.showForm.set(true);
    this.clearMessages();
  }

  closeForm(): void {
    this.showForm.set(false);
    this.isEditing.set(false);
    this.currentEditingId.set(null);
  }

  createPolicy(payload: CreateLeavePolicyRequest): void {
    this.clearMessages();
    this.service.createPolicy(payload).subscribe({
      next: () => {
        this.successMessage.set('Leave policy created successfully.');
        this.closeForm();
        this.loadAllPolicies();
      },
      error: (err) => {
        const errorMsg = err?.error?.title || err?.error?.message || 'Failed to create leave policy.';
        this.errorMessage.set(errorMsg);
         console.error(err)
      }
    });
  }

  updatePolicy(payload: UpdateLeavePolicyRequest): void {
    this.clearMessages();
    this.service.updatePolicy(payload).subscribe({
      next: () => {
        this.successMessage.set('Leave policy updated successfully.');
        this.closeForm();
        this.loadAllPolicies();
      },
      error: (err) => {
        const errorMsg = err?.error?.title || err?.error?.message || 'Failed to update leave policy.';
        this.errorMessage.set(errorMsg);
      }
    });
  }

  updateStatus(id: string, isEnabled: boolean): void {
    this.clearMessages();
    this.service.updatePolicyStatus({ id, isEnabled }).subscribe({
      next: () => {
        this.successMessage.set('Policy status updated successfully.');
        this.loadAllPolicies();
      },
      error: (err) => {
        const errorMsg = err?.error?.title || err?.error?.message || 'Failed to update status.';
        this.errorMessage.set(errorMsg);
      }
    });
  }

  private clearMessages(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }
}