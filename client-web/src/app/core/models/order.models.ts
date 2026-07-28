export enum OrderStatus {
  New = 0,
  Received = 1,
  Sorting = 2,
  Washing = 3,
  Drying = 4,
  Ironing = 5,
  Packing = 6,
  ReadyForDelivery = 7,
  Delivered = 8,
  Cancelled = 9
}

export enum PaymentStatus {
  Pending = 0,
  Partial = 1,
  Paid = 2,
  Refunded = 3
}

export interface OrderListItem {
  id: string;
  orderNumber: string;
  customerName: string;
  status: OrderStatus;
  totalAmount: number;
  paymentStatus: PaymentStatus;
  createdAtUtc: string;
}

export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
}

export interface CreateOrderItem {
  garmentId: string;
  serviceId: string;
  quantity: number;
  weightKg?: number;
  specialInstructions?: string;
}

export interface CreateOrderRequest {
  customerId: string;
  channel: number; // 0 WalkIn, 1 PickupRequest, 2 Express
  isExpress: boolean;
  items: CreateOrderItem[];
}

export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  userId: string;
  fullName: string;
  role: string;
}
