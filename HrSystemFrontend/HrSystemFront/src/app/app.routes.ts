import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/Auth/Components/login-form-component/login-form-component').then(c => c.LoginFormComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/Auth/Components/register-form-component/register-form-component').then(c => c.RegisterFormComponent),
    providers: []
  },
 
  {
    path: 'dashboard',
    loadComponent: () => 
      import('./features/dashboard/Components/dashboard-component/dashboard-component').then(m => m.DashboardComponent),
    providers: []
  },
  {
    path: 'leave-requests',
    loadComponent: () => 
      import('./features/LeaveRequest/Components/leave-request-component/leave-request-component').then(m => m.LeaveRequestComponent)
  },
  {
    path: 'team-requests',
    loadComponent: () => 
      import('./features/LeaveReview/Components/team-requests-component/team-requests-component').then(m => m.TeamRequestsComponent)
  },
  {
    path: 'leave-policies',
    loadComponent: () => 
      import('./features/LeavePolicy/Components/leave-policy-component/leave-policy-component').then(m => m.LeavePolicyComponent)
  },
  {
  path: 'public-holidays',
  loadComponent: () => import('./features/PublicHolidays/Components/public-holidays-component/public-holidays-component').then(m => m.PublicHolidaysComponent)
},
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  }
];