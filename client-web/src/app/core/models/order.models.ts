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
  Cancelled = 9,
  OutForDelivery = 10
}

export enum PaymentStatus {
  Pending = 0,
  Partial = 1,
  Paid = 2,
  Refunded = 3
}

export enum OrderChannel {
  WalkIn = 0,
  PickupRequest = 1,
  Express = 2
}

export interface OrderListItem {
  id: string;
  orderNumber: string;
  customerName: string;
  status: OrderStatus;
  totalAmount: number;
  paymentStatus: PaymentStatus;
  createdAtUtc: string;
  promisedByUtc?: string | null;
}

export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
}

export interface OrderItemDetail {
  id: string;
  garmentName: string;
  serviceName: string;
  quantity: number;
  weightKg?: number | null;
  unitPrice: number;
  lineTotal: number;
  specialInstructions?: string | null;
}

export interface OrderDetail {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  channel: OrderChannel;
  isExpress: boolean;
  subTotal: number;
  discountAmount: number;
  promoCode?: string | null;
  gstAmount: number;
  deliveryCharge: number;
  totalAmount: number;
  paymentStatus: PaymentStatus;
  amountPaid: number;
  createdAtUtc: string;
  promisedByUtc?: string | null;
  deliveredAtUtc?: string | null;
  pickupScheduledAtUtc?: string | null;
  items: OrderItemDetail[];
}

export interface CreateOrderItem {
  garmentId: string;
  serviceId: string;
  quantity: number;
  weightKg?: number;
  specialInstructions?: string;
}

export interface UpdateOrderRequest {
  status?: OrderStatus;
  paymentStatus?: PaymentStatus;
  amountPaid?: number;
  promisedByUtc?: string;
}

export interface CreateOrderRequest {
  customerId: string;
  channel: OrderChannel;
  isExpress: boolean;
  items: CreateOrderItem[];
  preferredPickupAtUtc?: string;
  pickupAddressId?: string;
  promoCode?: string;
}

export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  phoneNumber: string;
  email: string;
  password: string;
}

export interface VerifyOtpRequest {
  phoneNumber: string;
  code: string;
}

export interface RegisterResponse {
  userId: string;
  devOtpCode?: string;
}

export interface ForgotPasswordRequest {
  usernameOrEmail: string;
}

export interface ForgotPasswordResponse {
  accountFound: boolean;
  devOtpCode?: string;
}

export interface ResetPasswordRequest {
  usernameOrEmail: string;
  code: string;
  newPassword: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  userId: string;
  fullName: string;
  role: string;
}
