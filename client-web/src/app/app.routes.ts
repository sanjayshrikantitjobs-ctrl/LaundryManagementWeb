import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import {
  roleGuard,
  ADMIN_ONLY_ROLES,
  CUSTOMER_ONLY_ROLES,
  DELIVERY_AGENT_ROLES,
  MANAGEMENT_ROLES,
  PICKUP_AGENT_ROLES
} from './core/guards/role.guard';
import { HomeRedirectComponent } from './core/home-redirect/home-redirect.component';

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
    canActivate: [authGuard, roleGuard(MANAGEMENT_ROLES)],
    loadChildren: () => import('./features/customers/customers.routes').then((m) => m.CUSTOMERS_ROUTES)
  },
  {
    // No role guard at this level — garments.routes.ts guards its own children,
    // since /garments/pricing is viewable by Customers while the rest is management-only.
    path: 'garments',
    canActivate: [authGuard],
    loadChildren: () => import('./features/garments/garments.routes').then((m) => m.GARMENTS_ROUTES)
  },
  {
    path: 'services',
    canActivate: [authGuard, roleGuard(MANAGEMENT_ROLES)],
    loadChildren: () => import('./features/services/services.routes').then((m) => m.SERVICES_ROUTES)
  },
  {
    path: 'add-ons',
    canActivate: [authGuard, roleGuard(MANAGEMENT_ROLES)],
    loadChildren: () => import('./features/add-ons/add-ons.routes').then((m) => m.ADD_ONS_ROUTES)
  },
  {
    path: 'subscriptions',
    canActivate: [authGuard, roleGuard(MANAGEMENT_ROLES)],
    loadChildren: () => import('./features/subscriptions/subscriptions.routes').then((m) => m.SUBSCRIPTIONS_ROUTES)
  },
  {
    path: 'membership',
    canActivate: [authGuard, roleGuard(CUSTOMER_ONLY_ROLES)],
    loadComponent: () => import('./features/membership/membership.component').then((m) => m.MembershipComponent)
  },
  {
    path: 'shop',
    canActivate: [authGuard, roleGuard(CUSTOMER_ONLY_ROLES)],
    loadComponent: () => import('./features/shop/shop.component').then((m) => m.ShopComponent)
  },
  {
    path: 'services-info/:slug',
    canActivate: [authGuard, roleGuard(CUSTOMER_ONLY_ROLES)],
    loadComponent: () => import('./features/service-info/service-info.component').then((m) => m.ServiceInfoComponent)
  },
  {
    path: 'promotions',
    canActivate: [authGuard, roleGuard(CUSTOMER_ONLY_ROLES)],
    loadComponent: () => import('./features/promotions/promotions.component').then((m) => m.PromotionsComponent)
  },
  {
    path: 'my-requests',
    canActivate: [authGuard, roleGuard(CUSTOMER_ONLY_ROLES)],
    loadComponent: () => import('./features/my-requests/my-requests.component').then((m) => m.MyRequestsComponent)
  },
  {
    path: 'price-list',
    canActivate: [authGuard, roleGuard(CUSTOMER_ONLY_ROLES)],
    loadComponent: () => import('./features/price-list/price-list.component').then((m) => m.PriceListComponent)
  },
  {
    path: 'settings',
    canActivate: [authGuard, roleGuard(CUSTOMER_ONLY_ROLES)],
    loadComponent: () => import('./features/settings/settings.component').then((m) => m.SettingsComponent)
  },
  {
    path: 'contact-us',
    canActivate: [authGuard, roleGuard(CUSTOMER_ONLY_ROLES)],
    loadComponent: () => import('./features/contact-us/contact-us.component').then((m) => m.ContactUsComponent)
  },
  {
    path: 'admin/promotions',
    canActivate: [authGuard, roleGuard(MANAGEMENT_ROLES)],
    loadChildren: () => import('./features/admin-promotions/admin-promotions.routes').then((m) => m.ADMIN_PROMOTIONS_ROUTES)
  },
  {
    path: 'users',
    canActivate: [authGuard, roleGuard(ADMIN_ONLY_ROLES)],
    loadChildren: () => import('./features/users/users.routes').then((m) => m.USERS_ROUTES)
  },
  {
    path: 'pickup-queue',
    canActivate: [authGuard, roleGuard(PICKUP_AGENT_ROLES)],
    loadComponent: () =>
      import('./features/pickup-delivery/pickup-queue/pickup-queue.component').then((m) => m.PickupQueueComponent)
  },
  {
    path: 'delivery-queue',
    canActivate: [authGuard, roleGuard(DELIVERY_AGENT_ROLES)],
    loadComponent: () =>
      import('./features/pickup-delivery/delivery-queue/delivery-queue.component').then((m) => m.DeliveryQueueComponent)
  },
  { path: '', pathMatch: 'full', component: HomeRedirectComponent },
  { path: '**', component: HomeRedirectComponent }
];
