import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GarmentsService } from '../garments/garments.service';
import { ServicesService } from '../services/services.service';
import { PricingMatrix, PricingMatrixCell, PricingType, ServiceListItem } from '../../core/models/catalog.models';

enum ChannelType {
  WalkIn = 'WalkIn',
  Express = 'Express',
  PickupRequest = 'PickupRequest'
}

@Component({
  selector: 'app-price-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './price-list.component.html',
  styleUrl: './price-list.component.scss'
})
export class PriceListComponent implements OnInit {
  private readonly garmentsService = inject(GarmentsService);
  private readonly servicesService = inject(ServicesService);

  readonly ChannelType = ChannelType;
  readonly PricingType = PricingType;

  readonly matrix = signal<PricingMatrix | null>(null);
  readonly services = signal<ServiceListItem[]>([]);
  readonly isLoading = signal(true);
  readonly channel = signal<ChannelType>(ChannelType.WalkIn);

  readonly serviceById = computed(() => {
    const map = new Map<string, ServiceListItem>();
    for (const s of this.services()) map.set(s.id, s);
    return map;
  });

  ngOnInit(): void {
    this.servicesService.getServices({ pageSize: 50 }).subscribe((result) => this.services.set(result.items));
    this.garmentsService.getPricingMatrix().subscribe({
      next: (matrix) => {
        this.matrix.set(matrix);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  cellFor(row: { prices: PricingMatrixCell[] }, serviceId: string): PricingMatrixCell | undefined {
    return row.prices.find((c) => c.serviceId === serviceId);
  }

  displayPrice(basePrice: number, serviceId: string): number {
    if (this.channel() !== ChannelType.Express) return basePrice;
    const service = this.serviceById().get(serviceId);
    return basePrice + (service?.expressSurcharge ?? 0);
  }

  displayEta(serviceId: string): string {
    const service = this.serviceById().get(serviceId);
    if (!service) return '—';
    const hours = this.channel() === ChannelType.Express ? service.expressEtaHours : service.estimatedTimeHours;
    if (hours < 24) return `${hours} hr`;
    const days = Math.round(hours / 24);
    return `${days} day${days > 1 ? 's' : ''}`;
  }
}
