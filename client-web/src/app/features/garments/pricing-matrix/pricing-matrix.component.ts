import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { GarmentsService } from '../garments.service';
import { PricingMatrix, PricingType } from '../../../core/models/catalog.models';

@Component({
  selector: 'app-pricing-matrix',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './pricing-matrix.component.html',
  styleUrl: './pricing-matrix.component.scss'
})
export class PricingMatrixComponent implements OnInit {
  readonly matrix = signal<PricingMatrix | null>(null);
  readonly isLoading = signal(true);
  readonly savingKey = signal<string | null>(null);
  readonly PricingType = PricingType;

  constructor(private garmentsService: GarmentsService) {}

  ngOnInit(): void {
    this.load();
  }

  cellKey(garmentId: string, serviceId: string): string {
    return `${garmentId}:${serviceId}`;
  }

  onPriceChange(garmentId: string, serviceId: string, currentPricingType: PricingType | null, rawValue: string): void {
    const price = Number(rawValue);
    if (Number.isNaN(price) || price < 0) return;

    const key = this.cellKey(garmentId, serviceId);
    this.savingKey.set(key);
    this.garmentsService
      .setPrice(garmentId, serviceId, currentPricingType ?? PricingType.PerItem, price)
      .subscribe({
        next: () => {
          this.savingKey.set(null);
          this.load();
        },
        error: () => this.savingKey.set(null)
      });
  }

  private load(): void {
    this.isLoading.set(true);
    this.garmentsService.getPricingMatrix().subscribe({
      next: (matrix) => {
        this.matrix.set(matrix);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
