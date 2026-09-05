import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../Services/auth-service';
import { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from '../models/auth';
@Injectable({
  providedIn: 'root'
})
export class AuthFacade {
  private authService = inject(AuthService);
  private router = inject(Router);

  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly userRole =  signal<string | null>(null);
  login(credentials: LoginRequest): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    console.log(' Sending Login Request with:', credentials);

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        console.log('Success, Server Accepted the request:', response);
        localStorage.setItem('token', response.accessToken);
        localStorage.setItem('role',response.user.role)
        this.userRole.set(response.user.role);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading.set(false);
        console.error('Failed, Server rejected or error occurred:', err);
        this.errorMessage.set('Login failed. Please check your credentials.');
      }
    });
  }

  
  register(data: RegisterRequest): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.authService.register(data).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.successMessage.set(response.message);
        console.log('Registration success:', response);
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (err) => {
        this.isLoading.set(false);
        console.error('Registration failed:', err);
        this.errorMessage.set(err?.error?.message || 'Registration failed. Please try again.');
      }
    });
  }
  getAuthToken(): string | null {
    return localStorage.getItem('token');
  }
  getUserRole(): string | null {
  if (typeof window !== 'undefined') {
    return localStorage.getItem('role');
  }
  return null;
}
  logout(): void {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}
