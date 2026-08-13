import { Injectable, signal } from '@angular/core';

export interface ConfirmOptions {
  title: string;
  message: string;
  requireReason?: boolean;
  confirmLabel?: string;
  danger?: boolean;
}

export interface ConfirmResult {
  confirmed: boolean;
  reason?: string;
}

interface ConfirmRequest extends ConfirmOptions {
  resolve: (result: ConfirmResult) => void;
}

/// Promise-based confirm-with-reason dialog, replacing native window.confirm() across
/// the admin screens. Mounted once as <app-confirm-dialog /> in app.component.html;
/// callers just `await confirmDialogService.confirm({...})`.
@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  readonly request = signal<ConfirmRequest | null>(null);

  confirm(options: ConfirmOptions): Promise<ConfirmResult> {
    return new Promise((resolve) => {
      this.request.set({ ...options, resolve });
    });
  }
}
