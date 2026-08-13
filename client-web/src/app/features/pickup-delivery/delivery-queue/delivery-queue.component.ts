import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PickupDeliveryService } from '../pickup-delivery.service';
import { DeliveryStatus, MyPickupDelivery } from '../../../core/models/pickup-delivery.models';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';

@Component({
  selector: 'app-delivery-queue',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  templateUrl: './delivery-queue.component.html',
  styleUrl: './delivery-queue.component.scss'
})
export class DeliveryQueueComponent implements OnInit {
  readonly items = signal<MyPickupDelivery[]>([]);
  readonly isLoading = signal(true);
  readonly totalCount = signal(0);
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly DeliveryStatus = DeliveryStatus;

  constructor(private pickupDeliveryService: PickupDeliveryService) {}

  ngOnInit(): void {
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.load();
  }

  statusLabel(status: DeliveryStatus): string {
    return DeliveryStatus[status];
  }

  private load(): void {
    this.isLoading.set(true);
    this.pickupDeliveryService.getMine(this.pageNumber(), this.pageSize()).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
