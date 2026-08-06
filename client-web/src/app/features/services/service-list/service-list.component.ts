import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ServicesService } from '../services.service';
import { ServiceListItem } from '../../../core/models/catalog.models';

@Component({
  selector: 'app-service-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './service-list.component.html',
  styleUrl: './service-list.component.scss'
})
export class ServiceListComponent implements OnInit {
  readonly services = signal<ServiceListItem[]>([]);
  readonly isLoading = signal(true);
  readonly search = signal('');

  constructor(private servicesService: ServicesService) {}

  ngOnInit(): void {
    this.loadServices();
  }

  onSearch(term: string): void {
    this.search.set(term);
    this.loadServices();
  }

  deleteService(service: ServiceListItem): void {
    if (!confirm(`Delete service "${service.name}"?`)) return;

    this.servicesService.deleteService(service.id).subscribe({
      next: () => this.loadServices()
    });
  }

  private loadServices(): void {
    this.isLoading.set(true);
    this.servicesService.getServices({ search: this.search() || undefined }).subscribe({
      next: (result) => {
        this.services.set(result.items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
