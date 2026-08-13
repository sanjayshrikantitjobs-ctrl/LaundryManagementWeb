import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ContactMessagesService } from './contact-messages.service';
import { UploadService } from '../../core/services/upload.service';
import { ContactMessageStatus, ContactMessageType, MyContactMessage } from '../../core/models/contact-message.models';

@Component({
  selector: 'app-contact-us',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contact-us.component.html',
  styleUrl: './contact-us.component.scss'
})
export class ContactUsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly contactMessagesService = inject(ContactMessagesService);
  private readonly uploadService = inject(UploadService);

  readonly ContactMessageType = ContactMessageType;
  readonly ContactMessageStatus = ContactMessageStatus;

  readonly form = this.fb.group({
    type: [ContactMessageType.Feedback, Validators.required],
    message: ['', [Validators.required, Validators.maxLength(2000)]]
  });

  readonly imageUrl = signal<string | null>(null);
  readonly isUploading = signal(false);
  readonly isSubmitting = signal(false);
  readonly submitError = signal<string | null>(null);
  readonly submitted = signal(false);

  readonly myMessages = signal<MyContactMessage[]>([]);
  readonly isLoadingMessages = signal(true);

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages(): void {
    this.isLoadingMessages.set(true);
    this.contactMessagesService.getMine().subscribe({
      next: (messages) => {
        this.myMessages.set(messages);
        this.isLoadingMessages.set(false);
      },
      error: () => this.isLoadingMessages.set(false)
    });
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.isUploading.set(true);
    this.uploadService.uploadImage(file).subscribe({
      next: (result) => {
        this.imageUrl.set(result.url);
        this.isUploading.set(false);
      },
      error: () => this.isUploading.set(false)
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);
    const value = this.form.getRawValue();

    this.contactMessagesService
      .create({
        type: value.type!,
        message: value.message!,
        imageUrl: this.imageUrl() ?? undefined
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.submitted.set(true);
          this.form.reset({ type: ContactMessageType.Feedback, message: '' });
          this.imageUrl.set(null);
          this.loadMessages();
          setTimeout(() => this.submitted.set(false), 3000);
        },
        error: (err: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          this.submitError.set(err.error?.title ?? 'Failed to send your message.');
        }
      });
  }

  typeLabel(type: ContactMessageType): string {
    return { [ContactMessageType.Feedback]: 'Feedback', [ContactMessageType.Complaint]: 'Complaint', [ContactMessageType.Query]: 'Query' }[type];
  }
}
