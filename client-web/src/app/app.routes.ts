import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadChildren: () => import('./features/orders/orders.routes').then((m) => m.ORDERS_ROUTES)
  },
  {
    path: 'customers',
    canActivate: [authGuard],
    loadChildren: () => import('./features/customers/customers.routes').then((m) => m.CUSTOMERS_ROUTES)
  },
  {
    path: 'garments',
    canActivate: [authGuard],
    loadChildren: () => import('./features/garments/garments.routes').then((m) => m.GARMENTS_ROUTES)
  },
  {
    path: 'services',
    canActivate: [authGuard],
    loadChildren: () => import('./features/services/services.routes').then((m) => m.SERVICES_ROUTES)
  },
  { path: '', pathMatch: 'full', redirectTo: 'orders' },
  { path: '**', redirectTo: 'orders' }
];
