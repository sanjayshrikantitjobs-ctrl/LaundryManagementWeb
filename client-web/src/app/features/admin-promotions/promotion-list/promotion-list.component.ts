import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PromotionsService } from '../../promotions/promotions.service';
import { PromotionListItem } from '../../../core/models/promotion.models';

@Component({
  selector: 'app-promotion-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './promotion-list.component.html',
  styleUrl: './promotion-list.component.scss'
})
export class PromotionListComponent implements OnInit {
  private readonly promotionsService = inject(PromotionsService);

  readonly promotions = signal<PromotionListItem[]>([]);
  readonly isLoading = signal(true);

  ngOnInit(): void {
    this.loadPromotions();
  }

  deletePromotion(promotion: PromotionListItem): void {
    if (!confirm(`Delete promotion "${promotion.title}"?`)) return;

    this.promotionsService.deletePromotion(promotion.id).subscribe({
      next: () => this.loadPromotions()
    });
  }

  private loadPromotions(): void {
    this.isLoading.set(true);
    this.promotionsService.getPromotions().subscribe({
      next: (result) => {
        this.promotions.set(result.items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
