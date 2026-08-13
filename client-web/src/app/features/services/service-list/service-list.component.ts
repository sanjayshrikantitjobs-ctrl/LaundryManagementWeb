import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ServicesService } from '../services.service';
import { ServiceListItem } from '../../../core/models/catalog.models';
import { SortDirection } from '../../../core/models/order.models';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { AuthService } from '../../../core/services/auth.service';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-service-list',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, PaginationComponent],
  templateUrl: './service-list.component.html',
  styleUrl: './service-list.component.scss'
})
export class ServiceListComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly services = signal<ServiceListItem[]>([]);
  readonly isLoading = signal(true);
  readonly search = signal('');
  readonly totalCount = signal(0);
  readonly canEdit = !['Customer', 'DepartmentHead'].includes(this.authService.currentUser()?.role ?? '');

  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly sortBy = signal<string | null>(null);
  readonly sortDirection = signal<SortDirection>('asc');

  constructor(private servicesService: ServicesService) {}

  ngOnInit(): void {
    this.loadServices();
  }

  onSearch(term: string): void {
    this.search.set(term);
    this.pageNumber.set(1);
    this.loadServices();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadServices();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.loadServices();
  }

  sortByColumn(column: string): void {
    if (this.sortBy() === column) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(column);
      this.sortDirection.set('asc');
    }
    this.pageNumber.set(1);
    this.loadServices();
  }

  sortIndicator(column: string): string {
    if (this.sortBy() !== column) return '';
    return this.sortDirection() === 'asc' ? '▲' : '▼';
  }

  async deleteService(service: ServiceListItem): Promise<void> {
    const result = await this.confirmDialog.confirm({
      title: 'Delete service',
      message: `Delete service "${service.name}"? This cannot be undone.`,
      requireReason: true,
      confirmLabel: 'Delete',
      danger: true
    });
    if (!result.confirmed) return;

    this.servicesService.deleteService(service.id, result.reason).subscribe({
      next: () => this.loadServices()
    });
  }

  private loadServices(): void {
    this.isLoading.set(true);
    this.servicesService
      .getServices({
        search: this.search() || undefined,
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        sortBy: this.sortBy() ?? undefined,
        sortDirection: this.sortDirection()
      })
      .subscribe({
        next: (result) => {
          this.services.set(result.items);
          this.totalCount.set(result.totalCount);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
  }
}
