import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { GarmentsService } from '../garments.service';
import { GarmentListItem } from '../../../core/models/catalog.models';

@Component({
  selector: 'app-garment-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './garment-list.component.html',
  styleUrl: './garment-list.component.scss'
})
export class GarmentListComponent implements OnInit {
  readonly garments = signal<GarmentListItem[]>([]);
  readonly isLoading = signal(true);
  readonly search = signal('');

  constructor(private garmentsService: GarmentsService) {}

  ngOnInit(): void {
    this.loadGarments();
  }

  onSearch(term: string): void {
    this.search.set(term);
    this.loadGarments();
  }

  deleteGarment(garment: GarmentListItem): void {
    if (!confirm(`Delete garment "${garment.name}"?`)) return;

    this.garmentsService.deleteGarment(garment.id).subscribe({
      next: () => this.loadGarments()
    });
  }

  private loadGarments(): void {
    this.isLoading.set(true);
    this.garmentsService.getGarments({ search: this.search() || undefined }).subscribe({
      next: (result) => {
        this.garments.set(result.items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
