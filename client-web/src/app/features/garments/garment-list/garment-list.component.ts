import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { GarmentsService } from '../garments.service';
import { GarmentListItem } from '../../../core/models/catalog.models';
import { SortDirection } from '../../../core/models/order.models';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-garment-list',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  templateUrl: './garment-list.component.html',
  styleUrl: './garment-list.component.scss'
})
export class GarmentListComponent implements OnInit {
  private readonly authService = inject(AuthService);

  readonly garments = signal<GarmentListItem[]>([]);
  readonly isLoading = signal(true);
  readonly search = signal('');
  readonly totalCount = signal(0);
  readonly canEdit = !['Customer', 'DepartmentHead'].includes(this.authService.currentUser()?.role ?? '');

  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly sortBy = signal<string | null>(null);
  readonly sortDirection = signal<SortDirection>('asc');

  constructor(private garmentsService: GarmentsService) {}

  ngOnInit(): void {
    this.loadGarments();
  }

  onSearch(term: string): void {
    this.search.set(term);
    this.pageNumber.set(1);
    this.loadGarments();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadGarments();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.loadGarments();
  }

  sortByColumn(column: string): void {
    if (this.sortBy() === column) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(column);
      this.sortDirection.set('asc');
    }
    this.pageNumber.set(1);
    this.loadGarments();
  }

  sortIndicator(column: string): string {
    if (this.sortBy() !== column) return '';
    return this.sortDirection() === 'asc' ? '▲' : '▼';
  }

  deleteGarment(garment: GarmentListItem): void {
    if (!confirm(`Delete garment "${garment.name}"?`)) return;

    this.garmentsService.deleteGarment(garment.id).subscribe({
      next: () => this.loadGarments()
    });
  }

  private loadGarments(): void {
    this.isLoading.set(true);
    this.garmentsService
      .getGarments({
        search: this.search() || undefined,
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        sortBy: this.sortBy() ?? undefined,
        sortDirection: this.sortDirection()
      })
      .subscribe({
        next: (result) => {
          this.garments.set(result.items);
          this.totalCount.set(result.totalCount);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
  }
}
