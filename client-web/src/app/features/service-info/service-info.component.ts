import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SERVICE_INFO_CATEGORIES, ServiceInfoCategory } from './service-info.data';

@Component({
  selector: 'app-service-info',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './service-info.component.html',
  styleUrl: './service-info.component.scss'
})
export class ServiceInfoComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly categories = SERVICE_INFO_CATEGORIES;
  readonly slug = signal(this.route.snapshot.paramMap.get('slug') ?? SERVICE_INFO_CATEGORIES[0].slug);

  readonly category = computed<ServiceInfoCategory>(
    () => this.categories.find((c) => c.slug === this.slug()) ?? this.categories[0]
  );

  readonly whatsAppLink = 'https://wa.me/919867343302';

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug');
      if (slug) this.slug.set(slug);
    });
  }

  goToShop(): void {
    this.router.navigate(['/shop']);
  }
}
