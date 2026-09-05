import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeavePolicyFacade } from '../../../../core/Facade/leave-policy-facade';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LeavePolicyResponse } from '../../../../core/models/leave-policy';
import { Header } from '../../../../shared/components/header/header';
import { Footer } from '../../../../shared/components/footer/footer';
@Component({
  standalone: true,
  imports: [Footer, Header,CommonModule, ReactiveFormsModule],
  selector: 'app-leave-policy-component',
  styleUrl: './leave-policy-component.css',
  templateUrl: './leave-policy-component.html',
})
export class LeavePolicyComponent implements OnInit {
  readonly facade = inject(LeavePolicyFacade);
  private readonly fb = inject(FormBuilder);

  policyForm: FormGroup = this.fb.group({
    leaveType: ['', Validators.required],
    annualAllowance: [20, [Validators.required, Validators.min(0)]],
    maxConsecutiveDays: [10, [Validators.required, Validators.min(0)]],
    minNoticeDays: [3, [Validators.required, Validators.min(0)]],
    backdateDays: [0, [Validators.required, Validators.min(0)]]
  });

  ngOnInit(): void {
    this.facade.loadAllPolicies();
  }

  onOpenCreate(): void {
    this.policyForm.reset({
      leaveType: '',
      annualAllowance: 20,
      maxConsecutiveDays: 10,
      minNoticeDays: 3,
      backdateDays: 0
    });
    this.policyForm.get('leaveType')?.enable();
    this.facade.openCreateForm();
  }

  onOpenEdit(policy: LeavePolicyResponse): void {
    this.policyForm.patchValue({
      leaveType: policy.leaveType,
      annualAllowance: policy.annualAllowance,
      maxConsecutiveDays: policy.maxConsecutiveDays,
      minNoticeDays: policy.minNoticeDays,
      backdateDays: policy.backdateDays
    });
    this.policyForm.get('leaveType')?.disable();
    this.facade.openEditForm(policy);
  }

  onFilterChange(event: any): void {
    this.facade.setFilterText(event.target.value);
  }

  
  onStatusFilterChange(status: 'ALL' | 'ENABLED' | 'DISABLED'): void {
    this.facade.setStatusFilter(status);
  }

  onSubmit(): void {
    if (this.policyForm.invalid) return;

    if (this.facade.isEditing()) {
      const payload = {
        id: this.facade.currentEditingId()!,
        annualAllowance: this.policyForm.value.annualAllowance,
        maxConsecutiveDays: this.policyForm.value.maxConsecutiveDays,
        minNoticeDays: this.policyForm.value.minNoticeDays,
        backdateDays: this.policyForm.value.backdateDays
      };
      this.facade.updatePolicy(payload);
    } else {
      this.facade.createPolicy(this.policyForm.value);
    }
  }
}