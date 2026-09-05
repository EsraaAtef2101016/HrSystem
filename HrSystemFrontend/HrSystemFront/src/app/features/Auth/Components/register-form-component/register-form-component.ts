import { Component, inject } from '@angular/core';
import { UserRole } from '../../../../core/models/auth';
import { AuthFacade } from '../../../../core/Facade/auth-facade';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
@Component({
  imports: [CommonModule, ReactiveFormsModule],
  selector: 'app-register-form-component',
  styleUrl: './register-form-component.css',
  templateUrl: './register-form-component.html',
})
export class RegisterFormComponent {
  private readonly router = inject(Router);
navigateToLogin() {
  this.router.navigate(['/login']);
}
  private fb = inject(FormBuilder);
  readonly authFacade = inject(AuthFacade);

  
  roles = Object.values(UserRole).filter(val => typeof val === 'string');;
  registerForm: FormGroup = this.fb.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    userRole: ['Employee', [Validators.required]],
    managerId: [null]
  });

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.authFacade.register(this.registerForm.value);
    }
  }
}
