import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GarmentsService } from '../garments/garments.service';
import { ServicesService } from '../services/services.service';
import { PricingMatrix, PricingMatrixCell, PricingMatrixGarmentRow, PricingMatrixService, PricingType, ServiceListItem } from '../../core/models/catalog.models';

enum ChannelType {
  WalkIn = 'WalkIn',
  Express = 'Express',
  PickupRequest = 'PickupRequest'
}

interface ServiceGroup {
  categoryId: string;
  categoryName: string;
  services: PricingMatrixService[];
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
  readonly selectedCategoryId = signal<string | null>(null);

  readonly serviceById = computed(() => {
    const map = new Map<string, ServiceListItem>();
    for (const s of this.services()) map.set(s.id, s);
    return map;
  });

  readonly groupedServices = computed<ServiceGroup[]>(() => {
    const groups: ServiceGroup[] = [];
    for (const service of this.matrix()?.services ?? []) {
      let group = groups.find((g) => g.categoryId === service.categoryId);
      if (!group) {
        group = { categoryId: service.categoryId, categoryName: service.categoryName, services: [] };
        groups.push(group);
      }
      group.services.push(service);
    }
    return groups;
  });

  // Only the currently-selected category's services are shown as columns — with 12
  // categories and 2-3 services each, showing every category's columns at once made
  // the grid unusably wide.
  readonly visibleGroup = computed<ServiceGroup | undefined>(() =>
    this.groupedServices().find((g) => g.categoryId === this.selectedCategoryId())
  );

  // Show a garment row if it's the garment's own primary category, OR it already has
  // an active price under the selected category — garments are a shared pool (e.g.
  // "Boots" is priced under both Shoes & Footwear and Leather & Suede), and rows with
  // no price at all in this category would just be dashes anyway.
  readonly visibleRows = computed<PricingMatrixGarmentRow[]>(() => {
    const group = this.visibleGroup();
    if (!group) return [];
    const categoryId = this.selectedCategoryId();
    return (this.matrix()?.garments ?? []).filter((row) =>
      row.categoryId === categoryId || group.services.some((s) => this.cellFor(row, s.id) !== undefined)
    );
  });

  ngOnInit(): void {
    this.servicesService.getServices({ pageSize: 50 }).subscribe((result) => this.services.set(result.items));
    this.garmentsService.getPricingMatrix().subscribe({
      next: (matrix) => {
        this.matrix.set(matrix);
        this.isLoading.set(false);

        const groups: ServiceGroup[] = [];
        for (const service of matrix.services) {
          if (!groups.some((g) => g.categoryId === service.categoryId)) {
            groups.push({ categoryId: service.categoryId, categoryName: service.categoryName, services: [] });
          }
        }
        if (groups.length) this.selectedCategoryId.set(groups[0].categoryId);
      },
      error: () => this.isLoading.set(false)
    });
  }

  selectCategory(categoryId: string): void {
    this.selectedCategoryId.set(categoryId);
  }

  cellFor(row: { prices: PricingMatrixCell[] }, serviceId: string): PricingMatrixCell | undefined {
    const cell = row.prices.find((c) => c.serviceId === serviceId);
    return cell?.isActive && cell.price !== null ? cell : undefined;
  }

  displayPrice(cell: PricingMatrixCell): number {
    if (this.channel() !== ChannelType.Express) return cell.price ?? 0;
    if (cell.expressPrice !== null) return cell.expressPrice;
    const service = this.serviceById().get(cell.serviceId);
    return (cell.price ?? 0) + (service?.expressSurcharge ?? 0);
  }

  displayEta(cell: PricingMatrixCell): string {
    const service = this.serviceById().get(cell.serviceId);
    const isExpress = this.channel() === ChannelType.Express;
    const hours = isExpress
      ? cell.expressEtaHours ?? service?.expressEtaHours ?? 0
      : cell.estimatedTimeHours ?? service?.estimatedTimeHours ?? 0;
    if (hours < 24) return `${hours} hr`;
    const days = Math.round(hours / 24);
    return `${days} day${days > 1 ? 's' : ''}`;
  }
}
