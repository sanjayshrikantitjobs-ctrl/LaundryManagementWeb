import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { GarmentsService } from '../garments.service';
import { AuthService } from '../../../core/services/auth.service';
import { PricingMatrix, PricingMatrixCell, PricingMatrixGarmentRow, PricingMatrixService, PricingType, SetGarmentServicePriceRequest } from '../../../core/models/catalog.models';

interface ServiceGroup {
  categoryId: string;
  categoryName: string;
  services: PricingMatrixService[];
}

interface EditingCell {
  garmentId: string;
  garmentName: string;
  serviceId: string;
  serviceName: string;
}

@Component({
  selector: 'app-pricing-matrix',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ReactiveFormsModule],
  templateUrl: './pricing-matrix.component.html',
  styleUrl: './pricing-matrix.component.scss'
})
export class PricingMatrixComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly matrix = signal<PricingMatrix | null>(null);
  readonly isLoading = signal(true);
  readonly savingKey = signal<string | null>(null);
  readonly PricingType = PricingType;
  readonly canEdit = !['Customer', 'DepartmentHead'].includes(this.authService.currentUser()?.role ?? '');
  readonly selectedCategoryId = signal<string | null>(null);

  readonly editingCell = signal<EditingCell | null>(null);
  readonly editForm = this.fb.group({
    pricingType: [PricingType.PerItem],
    price: [0],
    expressPrice: this.fb.control<number | null>(null),
    gstPercentage: this.fb.control<number | null>(null),
    estimatedTimeHours: this.fb.control<number | null>(null),
    expressEtaHours: this.fb.control<number | null>(null),
    isActive: [true]
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

  // A garment row shows under a category if it's the garment's own primary category,
  // OR it already has a priced cell for one of that category's services — garments are
  // a shared pool (e.g. "Boots" is priced under both Shoes & Footwear and Leather &
  // Suede), so a strict category match alone would hide legitimately-priced cells.
  readonly visibleRows = computed<PricingMatrixGarmentRow[]>(() => {
    const group = this.visibleGroup();
    if (!group) return [];
    const categoryId = this.selectedCategoryId();
    return (this.matrix()?.garments ?? []).filter((row) =>
      row.categoryId === categoryId || group.services.some((s) => this.cellFor(row, s.id)?.price !== null)
    );
  });

  constructor(private garmentsService: GarmentsService) {}

  ngOnInit(): void {
    this.load();
  }

  selectCategory(categoryId: string): void {
    this.selectedCategoryId.set(categoryId);
  }

  cellKey(garmentId: string, serviceId: string): string {
    return `${garmentId}:${serviceId}`;
  }

  serviceName(serviceId: string): string {
    return this.matrix()?.services.find((s) => s.id === serviceId)?.name ?? '';
  }

  cellFor(row: { prices: PricingMatrixCell[] }, serviceId: string): PricingMatrixCell | undefined {
    return row.prices.find((c) => c.serviceId === serviceId);
  }

  onPriceChange(garmentId: string, cell: PricingMatrixCell, rawValue: string): void {
    const price = Number(rawValue);
    if (Number.isNaN(price) || price < 0) return;

    // Quick-edit path only changes Price — the request must still carry the cell's
    // existing overrides forward, otherwise the backend (which treats a missing JSON
    // property as "reset to default") would silently wipe them.
    this.savePrice(garmentId, cell.serviceId, {
      pricingType: cell.pricingType ?? PricingType.PerItem,
      price,
      expressPrice: cell.expressPrice,
      gstPercentage: cell.gstPercentage,
      estimatedTimeHours: cell.estimatedTimeHours,
      expressEtaHours: cell.expressEtaHours,
      isActive: cell.isActive
    });
  }

  openEditor(garmentId: string, garmentName: string, serviceName: string, cell: PricingMatrixCell): void {
    if (!this.canEdit) return;

    this.editingCell.set({ garmentId, garmentName, serviceId: cell.serviceId, serviceName });
    this.editForm.reset({
      pricingType: cell.pricingType ?? PricingType.PerItem,
      price: cell.price ?? 0,
      expressPrice: cell.expressPrice,
      gstPercentage: cell.gstPercentage,
      estimatedTimeHours: cell.estimatedTimeHours,
      expressEtaHours: cell.expressEtaHours,
      isActive: cell.isActive ?? true
    });
  }

  closeEditor(): void {
    this.editingCell.set(null);
  }

  saveEditor(): void {
    const ctx = this.editingCell();
    if (!ctx) return;

    const value = this.editForm.getRawValue();
    this.savePrice(ctx.garmentId, ctx.serviceId, {
      pricingType: value.pricingType!,
      price: value.price!,
      expressPrice: value.expressPrice,
      gstPercentage: value.gstPercentage,
      estimatedTimeHours: value.estimatedTimeHours,
      expressEtaHours: value.expressEtaHours,
      isActive: value.isActive!
    });
    this.closeEditor();
  }

  private savePrice(garmentId: string, serviceId: string, request: SetGarmentServicePriceRequest): void {
    const key = this.cellKey(garmentId, serviceId);
    this.savingKey.set(key);
    this.garmentsService.setPrice(garmentId, serviceId, request).subscribe({
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

        // Only default-select a category on the very first load — reloading after a
        // save (see savePrice below) must not reset whatever category the admin is
        // currently working in.
        if (this.selectedCategoryId() === null && matrix.services.length) {
          this.selectedCategoryId.set(matrix.services[0].categoryId);
        }
      },
      error: () => this.isLoading.set(false)
    });
  }
}
