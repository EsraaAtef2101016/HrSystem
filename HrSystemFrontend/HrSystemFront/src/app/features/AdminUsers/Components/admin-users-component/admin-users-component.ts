import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminUsersFacade } from '../../../../core/Facade/admin-users-facade';
import { FormsModule } from '@angular/forms';
import {EmployeeParticipationFacade} from '../../../../core/Facade/employee-participation-facade'
@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  selector: 'app-admin-users-component',
  styleUrl: './admin-users-component.css',
  templateUrl: './admin-users-component.html',
})
export class AdminUsersComponent implements OnInit {
  readonly facade = inject(AdminUsersFacade);
  readonly participationFacade = inject(EmployeeParticipationFacade);
  policyOptOutAllowed = true;
  policyCooldownDays = 7;
  ngOnInit(): void {
    this.facade.loadAllUsers();
  
  }
  refreshEmployeeStatus(employeeId: string): void {
  this.participationFacade.loadEmployeeStatus(employeeId);
}
  onFilterChange(event: any): void {
    this.facade.setFilterText(event.target.value);
  }

  onStatusFilterChange(status: 'ALL' | 'ACTIVE' | 'INACTIVE'): void {
    this.facade.setStatusFilter(status);
  }

  onChangeRole(userId: string, newRole: string): void {
    if (newRole) {
      this.facade.updateUserRole(userId, newRole);
    }
  }

  onToggleStatus(userId: string, currentStatus: boolean): void {
    this.facade.updateUserStatus(userId, !currentStatus);
  }
  onAssignManager(userId: string, managerId: string): void {
    if (managerId) {
      this.facade.updateUserManager(userId, managerId);
    }
  }
  onForceParticipation(employeeId: string, forceOptIn: boolean, reasonInput: HTMLInputElement): void {
    const reason = reasonInput.value.trim();
    if (!reason) {
      alert('Please enter a reason for forcing participation.');
      return;
    }
    this.participationFacade.forceParticipation(employeeId, forceOptIn, reason);
   setTimeout(() => {
    this.participationFacade.loadEmployeeStatus(employeeId);
  }, 400);
    reasonInput.value = '';
  }


  onSavePolicy(): void {
    this.participationFacade.updatePolicy(this.policyOptOutAllowed, Number(this.policyCooldownDays));
  }
}
