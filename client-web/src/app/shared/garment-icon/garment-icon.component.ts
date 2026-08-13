import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

// Maps the 12 service-category names onto the 6 pictograms below. Anything not
// listed here (Wash & Laundry, Ironing & Pressing, Dry Cleaning, Stain Removal,
// Alteration & Repair, Commercial / B2B Laundry) falls through to @default.
const CATEGORY_TO_ICON: Record<string, string> = {
  'shoes & footwear': 'footwear',
  'bags & accessories': 'bags',
  'bedding & blankets': 'household',
  'curtains & home furnishing': 'household',
  'wedding & traditional wear': 'womenswear',
  'leather & suede': 'menswear'
};

/// <summary>Hand-drawn line-art fallback icon shown when a garment has no uploaded
/// photo, picked by its catalog category (Bags, Footwear, Household, Menswear,
/// Womenswear, Woolen) so the shop grid reads as "shirt/dress/shoe/bag" at a glance
/// instead of one generic placeholder for every item.</summary>
@Component({
  selector: 'app-garment-icon',
  standalone: true,
  imports: [CommonModule],
  template: `
    <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg" [attr.aria-label]="category">
      @switch (normalizedCategory()) {
        @case ('bags') {
          <path d="M14 18c0-6 4.5-10 10-10s10 4 10 10" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"/>
          <rect x="8" y="18" width="32" height="22" rx="4" stroke="currentColor" stroke-width="2.4"/>
          <path d="M8 26h32" stroke="currentColor" stroke-width="1.6" opacity="0.5"/>
        }
        @case ('footwear') {
          <path d="M8 30c0-6 3-13 8-15 1.5 3 5 4 8 4h6c4 0 8 3.5 8 8v3H8z" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round"/>
          <path d="M8 33h30" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"/>
          <path d="M20 19c0 2.5 2 5 5 5" stroke="currentColor" stroke-width="1.6" opacity="0.5"/>
        }
        @case ('household') {
          <rect x="9" y="14" width="30" height="20" rx="3" stroke="currentColor" stroke-width="2.4"/>
          <path d="M9 20h30" stroke="currentColor" stroke-width="1.6" opacity="0.5"/>
          <path d="M16 26h16" stroke="currentColor" stroke-width="1.6" opacity="0.5"/>
        }
        @case ('menswear') {
          <path d="M18 8h12l4 6-6 4v22H20V18l-6-4z" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round"/>
          <path d="M18 8l-9 6 4 5" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round" stroke-linecap="round"/>
          <path d="M30 8l9 6-4 5" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round" stroke-linecap="round"/>
        }
        @case ('womenswear') {
          <path d="M20 8h8l3 6-3 3 6 23H14l6-23-3-3z" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round"/>
          <path d="M20 8l-7 5 3 4" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round" stroke-linecap="round"/>
          <path d="M28 8l7 5-3 4" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round" stroke-linecap="round"/>
        }
        @case ('woolen') {
          <path d="M17 9h14l3 7-5 3v21H19V19l-5-3z" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round"/>
          <path d="M17 9l-8 5 3 5" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round" stroke-linecap="round"/>
          <path d="M31 9l8 5-3 5" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round" stroke-linecap="round"/>
          <path d="M20 40h8M19 34h10" stroke="currentColor" stroke-width="1.6" opacity="0.5"/>
        }
        @default {
          <path d="M24 6v4" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"/>
          <path d="M14 10h20l4 6-8 5v19H18V21l-8-5z" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round"/>
        }
      }
    </svg>
  `,
  styles: [`
    :host { display: flex; align-items: center; justify-content: center; width: 100%; height: 100%; }
    svg { width: 56%; height: 56%; color: var(--color-primary); }
  `]
})
export class GarmentIconComponent {
  @Input() category = '';

  normalizedCategory(): string {
    const normalized = this.category.trim().toLowerCase();
    return CATEGORY_TO_ICON[normalized] ?? normalized;
  }
}
