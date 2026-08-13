import { Routes } from '@angular/router';
import { MANAGEMENT_ROLES, roleGuard } from '../../core/guards/role.guard';

export const ADD_ONS_ROUTES: Routes = [
  {
    path: '',
    canActivate: [roleGuard(MANAGEMENT_ROLES)],
    loadComponent: () => import('./add-on-list/add-on-list.component').then((m) => m.AddOnListComponent)
  },
  {
    path: 'new',
    canActivate: [roleGuard(MANAGEMENT_ROLES)],
    loadComponent: () => import('./add-on-form/add-on-form.component').then((m) => m.AddOnFormComponent)
  },
  {
    path: ':id/edit',
    canActivate: [roleGuard(MANAGEMENT_ROLES)],
    loadComponent: () => import('./add-on-form/add-on-form.component').then((m) => m.AddOnFormComponent)
  }
];
