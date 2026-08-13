export enum NotificationType {
  NewOrder = 0,
  OrderUpdated = 1,
  NewCustomerRegistered = 2
}

export interface AppNotification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  entityId?: string;
  isRead: boolean;
  createdAtUtc: string;
}
