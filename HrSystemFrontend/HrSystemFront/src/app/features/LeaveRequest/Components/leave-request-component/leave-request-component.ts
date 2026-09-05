import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestFacade } from '../../../../core/Facade/leave-request-facade';
import { CreateUpdateLeaveRequest, LeaveRequest } from '../../../../core/models/leave-request';

import { Header} from '../../../../shared/components/header/header';
import { Footer } from '../../../../shared/components/footer/footer';
@Component({
  imports: [Header,Footer,CommonModule, FormsModule, DatePipe],
  selector: 'app-leave-request-component',
  styleUrl: './leave-request-component.css',
  templateUrl: './leave-request-component.html',
})



export class LeaveRequestComponent implements OnInit {
  readonly leaveFacade = inject(LeaveRequestFacade);

  isEditing = false;
  editingId: string | null = null;

  formModel: CreateUpdateLeaveRequest = {
    leaveType: 'Vacation',
    startDate: '',
    endDate: ''
  };

  ngOnInit(): void {
    if (typeof window !== 'undefined') {
      this.leaveFacade.loadMyRequests();
    }
  }

  submitForm(): void {
    if (this.isEditing && this.editingId) {
      this.leaveFacade.updateLeaveRequest(this.editingId, this.formModel, () => {
        this.resetForm();
      });
    } else {
      this.leaveFacade.createLeaveRequest(this.formModel, () => {
        this.resetForm();
      });
    }
  }

  editRequest(req: any): void {
    this.isEditing = true;
    this.editingId = req.id;
    this.formModel = {
      leaveType: req.leaveType,
      startDate: req.startDate,
      endDate: req.endDate
    };
  }

  viewDetails(id: string): void {
    this.leaveFacade.loadRequestById(id);
  }
  
  canCancelRequest(req: any): boolean {
  return this.leaveFacade.canCancelRequest(req);
}
  resetForm(): void {
    this.isEditing = false;
    this.editingId = null;
    this.formModel = {
      leaveType: 'Vacation',
      startDate: '',
      endDate: ''
    };
  }
  cancelRequest(id: string): void {
  this.leaveFacade.cancelLeaveRequest(id);
}
}