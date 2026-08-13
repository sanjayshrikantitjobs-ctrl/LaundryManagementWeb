import { OrderStatus, PaymentStatus } from '../models/order.models';

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  [OrderStatus.New]: 'Accepted (Not Started)',
  [OrderStatus.Received]: 'Received at Store',
  [OrderStatus.Sorting]: 'Sorting',
  [OrderStatus.Washing]: 'Washing',
  [OrderStatus.Drying]: 'Drying',
  [OrderStatus.Ironing]: 'Ironing',
  [OrderStatus.Packing]: 'Packing',
  [OrderStatus.ReadyForDelivery]: 'Ready for Delivery',
  [OrderStatus.OutForDelivery]: 'Out for Delivery',
  [OrderStatus.Delivered]: 'Completed',
  [OrderStatus.Cancelled]: 'Cancelled'
};

export const TERMINAL_ORDER_STATUSES = new Set<OrderStatus>([OrderStatus.Delivered, OrderStatus.Cancelled]);

/// <summary>Simplified in-progress bucket for admins who don't need to distinguish
/// every internal pipeline step at a glance (order list tabs/filters).</summary>
export const IN_PROGRESS_ORDER_STATUSES = new Set<OrderStatus>([
  OrderStatus.Received, OrderStatus.Sorting, OrderStatus.Washing, OrderStatus.Drying,
  OrderStatus.Ironing, OrderStatus.Packing, OrderStatus.ReadyForDelivery, OrderStatus.OutForDelivery
]);

export function orderStatusLabel(status: OrderStatus): string {
  return ORDER_STATUS_LABELS[status] ?? 'Unknown';
}

export const PAYMENT_STATUS_LABELS: Record<PaymentStatus, string> = {
  [PaymentStatus.Pending]: 'Not Paid',
  [PaymentStatus.Partial]: 'Partially Paid',
  [PaymentStatus.Paid]: 'Paid',
  [PaymentStatus.Refunded]: 'Refunded'
};

export function paymentStatusLabel(status: PaymentStatus): string {
  return PAYMENT_STATUS_LABELS[status] ?? 'Unknown';
}
