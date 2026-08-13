import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConfirmDialogService } from './confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss'
})
export class ConfirmDialogComponent {
  private readonly service = inject(ConfirmDialogService);

  readonly request = this.service.request;
  readonly reason = signal('');

  get reasonMissing(): boolean {
    const req = this.request();
    return !!req?.requireReason && !this.reason().trim();
  }

  confirm(): void {
    const req = this.request();
    if (!req || this.reasonMissing) return;
    req.resolve({ confirmed: true, reason: this.reason().trim() || undefined });
    this.close();
  }

  cancel(): void {
    this.request()?.resolve({ confirmed: false });
    this.close();
  }

  private close(): void {
    this.service.request.set(null);
    this.reason.set('');
  }
}
