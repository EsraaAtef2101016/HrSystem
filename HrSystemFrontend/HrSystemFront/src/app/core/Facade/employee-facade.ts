import { Injectable, inject, signal } from '@angular/core';
import { EmployeeService } from '../Services/employee-service';
import { EmployeeParticipationStatus, LeaveBalance } from '../models/dashboard';


@Injectable({
  providedIn: 'root'
})
export class EmployeeFacade  {
  
  private readonly employeeService = inject(EmployeeService);

  readonly participationStatus = signal<EmployeeParticipationStatus | null>(null);
  readonly leaveBalances = signal<LeaveBalance[]>([]);

  
  loadEmployeeDashboardData(): void {
    this.employeeService.getParticipationStatus().subscribe({
      next: (data) =>{ this.participationStatus.set(data);
         console.log('Success, Server Accepted the request:')
         console.log("get Participation Status",data)
      },
      error: (err) => console.error('Error loading participation status', err)
    });

    this.employeeService.getCurrentLeaveBalances().subscribe({
      next: (data) => {this.leaveBalances.set(data)
         console.log("get Current LeaveBalances",data)
      },
      error: (err) => console.error('Error loading leave balances', err)
    });
  }
}