export enum DeliveryStatus {
  NotScheduled = 0,
  Scheduled = 1,
  OutForPickup = 2,
  PickedUp = 3,
  OutForDelivery = 4,
  Delivered = 5,
  Failed = 6
}

export interface MyPickupDelivery {
  pickupDeliveryId: string;
  orderId: string;
  orderNumber: string;
  customerName: string;
  addressLine?: string | null;
  status: DeliveryStatus;
  scheduledAtUtc?: string | null;
  completedAtUtc?: string | null;
}

export interface OrderPickupDelivery {
  id: string;
  isPickup: boolean;
  status: DeliveryStatus;
  scheduledAtUtc?: string | null;
  completedAtUtc?: string | null;
  assignedEmployeeId?: string | null;
  assignedEmployeeName?: string | null;
}

export interface Agent {
  employeeId: string;
  fullName: string;
}
