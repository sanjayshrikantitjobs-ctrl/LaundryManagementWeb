import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PromotionsService } from '../../promotions/promotions.service';
import { PromotionListItem } from '../../../core/models/promotion.models';
import { SortDirection } from '../../../core/models/order.models';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-promotion-list',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  templateUrl: './promotion-list.component.html',
  styleUrl: './promotion-list.component.scss'
})
export class PromotionListComponent implements OnInit {
  private readonly promotionsService = inject(PromotionsService);
  private readonly authService = inject(AuthService);

  readonly promotions = signal<PromotionListItem[]>([]);
  readonly isLoading = signal(true);
  readonly search = signal('');
  readonly totalCount = signal(0);
  readonly canEdit = !['Customer', 'DepartmentHead'].includes(this.authService.currentUser()?.role ?? '');

  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly sortBy = signal<string | null>(null);
  readonly sortDirection = signal<SortDirection>('desc');

  ngOnInit(): void {
    this.loadPromotions();
  }

  onSearch(term: string): void {
    this.search.set(term);
    this.pageNumber.set(1);
    this.loadPromotions();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadPromotions();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.loadPromotions();
  }

  sortByColumn(column: string): void {
    if (this.sortBy() === column) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(column);
      this.sortDirection.set('asc');
    }
    this.pageNumber.set(1);
    this.loadPromotions();
  }

  sortIndicator(column: string): string {
    if (this.sortBy() !== column) return '';
    return this.sortDirection() === 'asc' ? '▲' : '▼';
  }

  deletePromotion(promotion: PromotionListItem): void {
    if (!confirm(`Delete promotion "${promotion.title}"?`)) return;

    this.promotionsService.deletePromotion(promotion.id).subscribe({
      next: () => this.loadPromotions()
    });
  }

  private loadPromotions(): void {
    this.isLoading.set(true);
    this.promotionsService
      .getPromotions({
        search: this.search() || undefined,
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        sortBy: this.sortBy() ?? undefined,
        sortDirection: this.sortDirection()
      })
      .subscribe({
        next: (result) => {
          this.promotions.set(result.items);
          this.totalCount.set(result.totalCount);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
  }
}
