import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmployeeParticipationFacade } from '../../../../core/Facade/employee-participation-facade';

@Component({
  standalone: true,
  imports: [CommonModule],
  selector: 'app-participation-card-component',
  styleUrl: './participation-card-component.css',
  templateUrl: './participation-card-component.html',
})
export class ParticipationCardComponent implements OnInit {
  readonly facade = inject(EmployeeParticipationFacade);

  ngOnInit(): void {
    this.facade.loadStatus();
  }
  
  isCooldownActive(cooldownDate: string | null): boolean {
    if (!cooldownDate) return false;
    return new Date() < new Date(cooldownDate);
  }

  onToggle(isCurrentlyOptedIn: boolean): void {
    const actionName = isCurrentlyOptedIn ? 'opt out' : 'opt in';
    if (confirm(`Are you sure you want to ${actionName}?`)) {
      if (isCurrentlyOptedIn) {
        this.facade.optOut();
      } else {
        this.facade.optIn();
      }
    }
  }
}