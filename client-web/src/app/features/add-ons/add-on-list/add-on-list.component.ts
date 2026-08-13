import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AddOnsService } from '../add-ons.service';
import { AddOn } from '../../../core/models/catalog.models';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-add-on-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './add-on-list.component.html',
  styleUrl: './add-on-list.component.scss'
})
export class AddOnListComponent implements OnInit {
  private readonly addOnsService = inject(AddOnsService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly addOns = signal<AddOn[]>([]);
  readonly isLoading = signal(true);

  ngOnInit(): void {
    this.loadAddOns();
  }

  async deleteAddOn(addOn: AddOn): Promise<void> {
    const result = await this.confirmDialog.confirm({
      title: 'Delete add-on',
      message: `Delete add-on "${addOn.name}"? This cannot be undone.`,
      requireReason: true,
      confirmLabel: 'Delete',
      danger: true
    });
    if (!result.confirmed) return;

    this.addOnsService.deleteAddOn(addOn.id, result.reason).subscribe({
      next: () => this.loadAddOns()
    });
  }

  private loadAddOns(): void {
    this.isLoading.set(true);
    this.addOnsService.getAddOns().subscribe({
      next: (result) => {
        this.addOns.set(result);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
