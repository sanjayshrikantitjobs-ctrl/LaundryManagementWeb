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
  { path: '', pathMatch: 'full', redirectTo: 'orders' },
  { path: '**', redirectTo: 'orders' }
];
