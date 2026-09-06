import { Component, HostListener, inject, PLATFORM_ID } from '@angular/core';
import { AuthFacade } from '../../../core/Facade/auth-facade';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule } from '@angular/router';
import { EmployeeParticipationFacade } from '../../../core/Facade/employee-participation-facade';

@Component({
  imports: [CommonModule, RouterModule],
  selector: 'app-header',
  styleUrl: './header.css',
  templateUrl: './header.html',
})
export class Header {
  private authFacade = inject(AuthFacade);
  private platformId = inject(PLATFORM_ID);
  readonly facade = inject(EmployeeParticipationFacade);

  readonly userRole = isPlatformBrowser(this.platformId)
    ? localStorage.getItem('role')?.toLowerCase() || ''
    : '';

  get isAdmin(): boolean { return this.userRole === 'admin'; }
  get isManager(): boolean { return this.userRole === 'manager'; }
  get isEmployee(): boolean { return this.userRole === 'employee'; }

  isMobileMenuOpen = false;
  isDropdownOpen = false;
  isModalOpen = false;

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }

  toggleDropdown(event: Event): void {
    event.stopPropagation();
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  @HostListener('document:click')
  closeDropdown(): void {
    this.isDropdownOpen = false;
  }

  openModal(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.isModalOpen = true;
    this.isDropdownOpen = false;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  onLogout(): void {
    this.isDropdownOpen = false;
    // Add your logout logic here
  }

  isCooldownActive(cooldownDate: string | null): boolean {
    console.log(cooldownDate)
    if (!cooldownDate) return false;
    return new Date() < new Date(cooldownDate);
  }

  confirmToggleStatus(): void {
  const isCurrentlyOptedIn = this.facade.status()?.isOptedIn;
  
  if (isCurrentlyOptedIn) {
    this.facade.optOut(() => this.closeModal());
  } else {
    this.facade.optIn(() => this.closeModal());
  }
}
}