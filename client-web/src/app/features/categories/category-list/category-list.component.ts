import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CategoriesService } from '../categories.service';
import { ServiceCategory } from '../../../core/models/catalog.models';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './category-list.component.html',
  styleUrl: './category-list.component.scss'
})
export class CategoryListComponent implements OnInit {
  private readonly categoriesService = inject(CategoriesService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly categories = signal<ServiceCategory[]>([]);
  readonly isLoading = signal(true);

  ngOnInit(): void {
    this.loadCategories();
  }

  async deleteCategory(category: ServiceCategory): Promise<void> {
    const result = await this.confirmDialog.confirm({
      title: 'Delete category',
      message: `Delete category "${category.name}"? This cannot be undone.`,
      requireReason: true,
      confirmLabel: 'Delete',
      danger: true
    });
    if (!result.confirmed) return;

    this.categoriesService.deleteCategory(category.id, result.reason).subscribe({
      next: () => this.loadCategories(),
      error: (err) => alert(err.error?.title ?? 'Failed to delete category — it may still have services assigned.')
    });
  }

  private loadCategories(): void {
    this.isLoading.set(true);
    this.categoriesService.getCategories().subscribe({
      next: (result) => {
        this.categories.set(result);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
