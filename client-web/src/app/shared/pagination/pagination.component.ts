import { Component, computed, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export const PAGE_SIZE_OPTIONS = [10, 20, 50] as const;

/// <summary>Generic server-side pagination bar — reused across every paginated list
/// in the app instead of each component reinventing page-size/next-prev/page-number
/// controls. Purely presentational: the parent owns pageNumber/pageSize/totalCount and
/// re-fetches from the server when (pageChange)/(pageSizeChange) fire.</summary>
@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pagination.component.html',
  styleUrl: './pagination.component.scss'
})
export class PaginationComponent {
  readonly pageNumber = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly totalCount = input.required<number>();

  readonly pageChange = output<number>();
  readonly pageSizeChange = output<number>();

  readonly pageSizeOptions = PAGE_SIZE_OPTIONS;

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  readonly startItem = computed(() => (this.totalCount() === 0 ? 0 : (this.pageNumber() - 1) * this.pageSize() + 1));
  readonly endItem = computed(() => Math.min(this.pageNumber() * this.pageSize(), this.totalCount()));

  /// Windowed page-number list with -1 markers for ellipsis gaps, e.g. [1, -1, 4, 5, 6, -1, 13].
  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.pageNumber();
    const window = 1;
    const pages: number[] = [];

    const addRange = (from: number, to: number) => {
      for (let i = from; i <= to; i++) pages.push(i);
    };

    if (total <= 7) {
      addRange(1, total);
      return pages;
    }

    pages.push(1);
    if (current - window > 2) pages.push(-1);
    addRange(Math.max(2, current - window), Math.min(total - 1, current + window));
    if (current + window < total - 1) pages.push(-1);
    pages.push(total);

    return pages;
  });

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.pageNumber()) return;
    this.pageChange.emit(page);
  }

  onPageSizeChange(size: number): void {
    this.pageSizeChange.emit(+size);
  }
}
