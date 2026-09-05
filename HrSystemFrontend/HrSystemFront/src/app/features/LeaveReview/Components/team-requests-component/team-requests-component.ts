import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeaveReviewFacade } from '../../../../core/Facade/leave-review-facade';
import { Header } from '../../../../shared/components/header/header';
import { Footer } from '../../../../shared/components/footer/footer';
@Component({
  standalone: true,
  imports: [Footer, Header,CommonModule],
  selector: 'app-team-requests-component',
  styleUrl: './team-requests-component.css',
  templateUrl: './team-requests-component.html',
})
export class TeamRequestsComponent implements OnInit {
  readonly facade = inject(LeaveReviewFacade);

  ngOnInit(): void {
    this.facade.loadPendingRequests();
  }

  approve(id: string): void {
    this.facade.acceptRequest(id);
  }

  reject(id: string): void {
    const reason = prompt('Please enter rejection reason:');
    if (!reason) return;
    
    this.facade.rejectRequest(id, { rejectionReason: reason });
  }
}
