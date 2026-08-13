import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PromotionsService } from './promotions.service';
import { ActivePromotion } from '../../core/models/promotion.models';

@Component({
  selector: 'app-promotions',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './promotions.component.html',
  styleUrl: './promotions.component.scss'
})
export class PromotionsComponent implements OnInit {
  private readonly promotionsService = inject(PromotionsService);

  readonly promotions = signal<ActivePromotion[]>([]);
  readonly isLoading = signal(true);
  readonly copiedCode = signal<string | null>(null);

  ngOnInit(): void {
    this.promotionsService.getActivePromotions().subscribe({
      next: (promos) => {
        this.promotions.set(promos);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  copyCode(code: string): void {
    navigator.clipboard?.writeText(code);
    this.copiedCode.set(code);
    setTimeout(() => this.copiedCode.set(null), 1500);
  }
}
