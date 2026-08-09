import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent)
  },
  {
    path: 'verify-otp',
    loadComponent: () => import('./features/auth/verify-otp/verify-otp.component').then((m) => m.VerifyOtpComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password.component').then((m) => m.ForgotPasswordComponent)
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
  {
    path: 'shop',
    canActivate: [authGuard],
    loadComponent: () => import('./features/shop/shop.component').then((m) => m.ShopComponent)
  },
  {
    path: 'services-info/:slug',
    canActivate: [authGuard],
    loadComponent: () => import('./features/service-info/service-info.component').then((m) => m.ServiceInfoComponent)
  },
  {
    path: 'promotions',
    canActivate: [authGuard],
    loadComponent: () => import('./features/promotions/promotions.component').then((m) => m.PromotionsComponent)
  },
  {
    path: 'my-requests',
    canActivate: [authGuard],
    loadComponent: () => import('./features/my-requests/my-requests.component').then((m) => m.MyRequestsComponent)
  },
  {
    path: 'price-list',
    canActivate: [authGuard],
    loadComponent: () => import('./features/price-list/price-list.component').then((m) => m.PriceListComponent)
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    loadComponent: () => import('./features/settings/settings.component').then((m) => m.SettingsComponent)
  },
  {
    path: 'contact-us',
    canActivate: [authGuard],
    loadComponent: () => import('./features/contact-us/contact-us.component').then((m) => m.ContactUsComponent)
  },
  {
    path: 'admin/promotions',
    canActivate: [authGuard],
    loadChildren: () => import('./features/admin-promotions/admin-promotions.routes').then((m) => m.ADMIN_PROMOTIONS_ROUTES)
  },
  { path: '', pathMatch: 'full', redirectTo: 'orders' },
  { path: '**', redirectTo: 'orders' }
];
