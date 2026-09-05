import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { EmployeeFacade } from '../../../../core/Facade/employee-facade';
import{LeaveRequestFacade} from'../../../../core/Facade/leave-request-facade'
import { LeaveType } from '../../../../core/models/dashboard';
import {ParticipationCardComponent} from '../../../Participation/Components/participation-card-component/participation-card-component'
@Component({
  imports: [CommonModule, DatePipe,ParticipationCardComponent ],
  selector: 'app-employee-dashboard-details-component',
  styleUrl: './employee-dashboard-details-component.css',
  templateUrl: './employee-dashboard-details-component.html',
})


export class EmployeeDashboardDetailsComponent implements OnInit {
  readonly employeeFacade = inject(EmployeeFacade);
  readonly leaveRequestFacade = inject(LeaveRequestFacade);
  ngOnInit(): void {
    this.employeeFacade.loadEmployeeDashboardData();
    this.leaveRequestFacade.loadMyRequests();
  }

  formatLeaveType(type: LeaveType): string {
    switch (type) {
      case LeaveType.Vacation: 
        return 'Vacation';
      case LeaveType.DayOff: 
        return 'Day Off';
      case LeaveType.SickLeave: 
        return 'Sick Leave';
      default: 
        return type;
    }
  }
}