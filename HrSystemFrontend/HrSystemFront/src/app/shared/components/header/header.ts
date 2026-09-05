import { Component, inject, PLATFORM_ID } from '@angular/core';
import { AuthFacade } from '../../../core/Facade/auth-facade';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  imports: [CommonModule, RouterModule],
  selector: 'app-header',
  styleUrl: './header.css',
  templateUrl: './header.html',
})
export class Header {
  private authFacade = inject(AuthFacade);
  private platformId = inject(PLATFORM_ID);

  readonly userRole = isPlatformBrowser(this.platformId)
    ? localStorage.getItem('role')?.toLowerCase() || ''
    : '';

  onLogout(): void {
    this.authFacade.logout();
  }

  get isAdmin(): boolean { return this.userRole === 'admin'; }
  get isManager(): boolean { return this.userRole === 'manager'; }
  get isEmployee(): boolean { return this.userRole === 'employee'; }

  isMobileMenuOpen = false;

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }
}