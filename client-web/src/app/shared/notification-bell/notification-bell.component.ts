import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription, interval, startWith, switchMap } from 'rxjs';
import { NotificationsService } from '../../core/services/notifications.service';
import { AppNotification, NotificationType } from '../../core/models/notification.models';

const POLL_INTERVAL_MS = 25000;

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.scss'
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  private readonly notificationsService = inject(NotificationsService);
  private readonly router = inject(Router);

  readonly unreadCount = signal(0);
  readonly isOpen = signal(false);
  readonly notifications = signal<AppNotification[]>([]);
  readonly isLoading = signal(false);

  private pollSub?: Subscription;

  ngOnInit(): void {
    this.pollSub = interval(POLL_INTERVAL_MS)
      .pipe(
        startWith(0),
        switchMap(() => this.notificationsService.getUnreadCount())
      )
      .subscribe((count) => this.unreadCount.set(count));
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
  }

  toggle(): void {
    this.isOpen.update((v) => !v);
    if (this.isOpen()) {
      this.isLoading.set(true);
      this.notificationsService.getMine().subscribe({
        next: (list) => {
          this.notifications.set(list);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  close(): void {
    this.isOpen.set(false);
  }

  open(notification: AppNotification): void {
    if (!notification.isRead) {
      this.notificationsService.markRead(notification.id).subscribe(() => {
        this.notifications.update((list) =>
          list.map((n) => (n.id === notification.id ? { ...n, isRead: true } : n))
        );
        this.unreadCount.update((c) => Math.max(0, c - 1));
      });
    }

    this.close();

    if (!notification.entityId) return;

    if (notification.type === NotificationType.NewCustomerRegistered) {
      this.router.navigate(['/customers', notification.entityId, 'edit']);
    } else {
      this.router.navigate(['/orders', notification.entityId]);
    }
  }
}
