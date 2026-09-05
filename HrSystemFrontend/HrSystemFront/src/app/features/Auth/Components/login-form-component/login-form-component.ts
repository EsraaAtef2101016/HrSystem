// login-form-component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthFacade } from '../../../../core/Facade/auth-facade';
import { Router } from '@angular/router';
@Component({
  selector: 'app-login-form-component',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login-form-component.html',
  styleUrl: './login-form-component.css'
})
export class LoginFormComponent {
  private fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private authFacade = inject(AuthFacade);

  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  isLoading = this.authFacade.isLoading;
  errorMessage = this.authFacade.errorMessage;

  onLogin() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const { email, password } = this.loginForm.value;
    this.authFacade.login({ email, password });
  }
  navigateToRegister(): void {
    this.router.navigate(['/register']);
  }
}