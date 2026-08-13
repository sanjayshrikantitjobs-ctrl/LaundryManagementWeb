export enum BillingCycle {
  Monthly = 0,
  Quarterly = 1,
  Yearly = 2,
  HalfYearly = 3
}

export enum SubscriptionStatus {
  Active = 0,
  Paused = 1,
  Cancelled = 2,
  Expired = 3
}

export interface SubscriptionPlanFeature {
  id: string;
  text: string;
  displayOrder: number;
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  description?: string;
  billingCycle: BillingCycle;
  garmentsPerCycle: number;
  price: number;
  displayOrder: number;
  isActive: boolean;
  features: SubscriptionPlanFeature[];
}

export interface CreateSubscriptionPlanRequest {
  name: string;
  description?: string;
  billingCycle: BillingCycle;
  garmentsPerCycle: number;
  price: number;
  displayOrder: number;
  features: string[];
}

export interface UpdateSubscriptionPlanRequest extends CreateSubscriptionPlanRequest {
  isActive: boolean;
}

export interface CustomerSubscriptionListItem {
  id: string;
  customerId: string;
  customerName: string;
  subscriptionPlanId: string;
  planName: string;
  recurringValue: number;
  startDate: string;
  endDate: string;
  nextBillingDate?: string;
  status: SubscriptionStatus;
  notes?: string;
}

export type CustomerSubscriptionDetail = CustomerSubscriptionListItem;

export interface AssignCustomerSubscriptionRequest {
  customerId: string;
  subscriptionPlanId: string;
  recurringValue: number;
  startDate: string;
  notes?: string;
}

export interface UpdateCustomerSubscriptionRequest {
  recurringValue: number;
  status: SubscriptionStatus;
  nextBillingDate?: string | null;
  notes?: string;
}
