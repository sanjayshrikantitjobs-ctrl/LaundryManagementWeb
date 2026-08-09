import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { NotificationBellComponent } from './shared/notification-bell/notification-bell.component';
import { SERVICE_INFO_CATEGORIES } from './features/service-info/service-info.data';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NotificationBellComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isMenuOpen = signal(false);
  readonly isServicesMenuOpen = signal(false);
  readonly serviceCategories = SERVICE_INFO_CATEGORIES;

  get isCustomer(): boolean {
    return this.authService.currentUser()?.role === 'Customer';
  }

  closeMenu(): void {
    this.isMenuOpen.set(false);
    this.isServicesMenuOpen.set(false);
  }

  toggleMenu(): void {
    this.isMenuOpen.update((v) => !v);
  }

  toggleServicesMenu(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.isServicesMenuOpen.update((v) => !v);
  }

  logout(): void {
    this.closeMenu();
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
