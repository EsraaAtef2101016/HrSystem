import { Component, inject, computed } from '@angular/core';
import { Header } from '../../../../shared/components/header/header';
import { Footer } from '../../../../shared/components/footer/footer';
import { AuthFacade } from '../../../../core/Facade/auth-facade';
import { EmployeeDashboardDetailsComponent } from '../employee-dashboard-details-component/employee-dashboard-details-component';
import { AdminUsersComponent } from '../../../AdminUsers/Components/admin-users-component/admin-users-component'; // عدلي المسار حسب مكان الكومبوننت الخاص بالآدمن

@Component({
  standalone: true,
  imports: [Footer, Header, EmployeeDashboardDetailsComponent, AdminUsersComponent],
  selector: 'app-dashboard-component',
  styleUrl: './dashboard-component.css',
  templateUrl: './dashboard-component.html',
})
export class DashboardComponent {
  readonly authFacade = inject(AuthFacade);
  
  readonly userRole = computed(() => {
    if (typeof window === 'undefined') {
      return ''; 
    }
    return localStorage.getItem('role')?.toLowerCase() || '';
  });

  readonly isAdmin = computed(() => this.userRole() === 'admin');
  readonly isEmployeeOrManager = computed(() => {
    const role = this.userRole();
    return role === 'employee' || role === 'manager';
  });
}