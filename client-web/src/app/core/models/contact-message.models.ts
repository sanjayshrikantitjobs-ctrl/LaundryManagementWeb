export enum ContactMessageType {
  Feedback = 0,
  Complaint = 1,
  Query = 2
}

export enum ContactMessageStatus {
  Open = 0,
  Resolved = 1
}

export interface MyContactMessage {
  id: string;
  type: ContactMessageType;
  message: string;
  imageUrl?: string;
  status: ContactMessageStatus;
  response?: string;
  createdAtUtc: string;
}

export interface CreateContactMessageRequest {
  type: ContactMessageType;
  message: string;
  imageUrl?: string;
}
