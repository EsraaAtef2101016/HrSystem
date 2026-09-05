import { inject, Injectable, signal, computed } from '@angular/core';
import { UserProfileResponse } from '../models/user-profile';
import { AdminUsersService } from '../Services/admin-users-service';

@Injectable({ providedIn: 'root' })
export class AdminUsersFacade {
  private readonly service = inject(AdminUsersService);

  readonly users = signal<UserProfileResponse[]>([]);
  readonly Admins = signal<UserProfileResponse[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly showAdminsOnly = signal<boolean>(false);

  readonly filterText = signal<string>('');
  readonly statusFilter = signal<'ALL' | 'ACTIVE' | 'INACTIVE'>('ALL');

  readonly activeList = computed(() => this.showAdminsOnly() ? this.Admins() : this.users());

  readonly totalCount = computed(() => this.activeList().length);
  readonly activeCount = computed(() => this.activeList().filter(u => u.isActive).length);
  readonly inactiveCount = computed(() => this.activeList().filter(u => !u.isActive).length);

  readonly filteredUsers = computed(() => {
    const text = this.filterText().toLowerCase().trim();
    const status = this.statusFilter();
    const currentList = this.activeList();

    return currentList.filter(u => {
      const matchesText = !text || 
        String(u.name).toLowerCase().includes(text) ||
        String(u.email).toLowerCase().includes(text) ||
        String(u.role).toLowerCase().includes(text);

      const matchesStatus = 
        status === 'ALL' || 
        (status === 'ACTIVE' && u.isActive) || 
        (status === 'INACTIVE' && !u.isActive);

      return matchesText && matchesStatus;
    });
  });

  toggleViewMode(): void {
    const currentState = this.showAdminsOnly();
    this.showAdminsOnly.set(!currentState);
    this.clearMessages();

    if (!currentState) {
      if (this.Admins().length === 0) {
        this.loadAllAdminsUsers();
      }
    } else {
      if (this.users().length === 0) {
        this.loadAllUsers();
      }
    }
  }

  setFilterText(text: string): void {
    this.filterText.set(text);
  }

  setStatusFilter(status: 'ALL' | 'ACTIVE' | 'INACTIVE'): void {
    this.statusFilter.set(status);
  }

  loadAllUsers(): void {
    this.isLoading.set(true);
    this.clearMessages();

    this.service.getAllUsers().subscribe({
      next: (data) => {
        this.users.set(data);
        this.isLoading.set(false);
        console.log( "get All Users response ",data)
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to load users.');
        this.isLoading.set(false);
        console.error( "Error",err)
      }
    });
  }

  loadAllAdminsUsers(): void {
    this.isLoading.set(true);
    this.clearMessages();

    this.service.getAllAdminUsers().subscribe({
      next: (data) => {
        this.Admins.set(data);
        this.isLoading.set(false);
         console.log( "get All Admin response ",data)
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message || 'Failed to load admin users.');
        this.isLoading.set(false);
        console.error( "Error",err)
      }
    });
  }

  updateUserRole(id: string, userRole: string): void {
    this.clearMessages();
    this.service.updateRole({ id, userRole }).subscribe({
      next: (updatedUser: UserProfileResponse) => {
        
        this.successMessage.set('User role updated successfully.');
        this.users.update(list => list.map(u => u.id === id ? updatedUser : u));
        console.log( "update User Role ",updatedUser);
      },
      error: (err) => {this.errorMessage.set(err?.error?.message || 'Failed to update role.');
         console.error( "Error",err);
      }
    });
  }

  updateUserStatus(id: string, isActive: boolean): void {
    this.clearMessages();
    this.service.updateStatus({ id, isActive }).subscribe({
      next: (updatedUser: UserProfileResponse) => {
        this.successMessage.set('User status updated successfully.');
        this.users.update(list => list.map(u => u.id === id ? updatedUser : u));
        console.log( "update User Status ",updatedUser)
      },
      error: (err) => {this.errorMessage.set(err?.error?.message || 'Failed to update status.');
      console.error( "Error",err);
    
  }});
  }

  updateUserManager(id: string, managerId: string): void {
    this.clearMessages();
    this.service.updateManager({ id, managerId }).subscribe({
      next: (updatedUser: UserProfileResponse) => {
        this.successMessage.set('Manager assigned successfully.');
        this.users.update(list => list.map(u => u.id === id ? updatedUser : u));
        console.log( "update User Manager ",updatedUser)
      },
      error: (err) => {this.errorMessage.set(err?.error?.message || 'Failed to assign manager.');
        console.error( "Error",err);}
    });
  }

  private clearMessages(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  getManagerName(managerId: string | null): string {
    if (!managerId) return 'No Manager Assigned';
    const manager = this.users().find(u => u.id === managerId);
    return manager ? manager.name : 'No Manager Assigned';
  }

  getManagerEmail(managerId: string | null): string {
    if (!managerId) return 'No Manager Assigned';
    const manager = this.users().find(u => u.id === managerId);
    return manager ? manager.email : 'No Manager Assigned';
  }
}