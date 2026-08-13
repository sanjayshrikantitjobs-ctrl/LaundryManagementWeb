export enum MembershipTier {
  None = 0,
  Silver = 1,
  Gold = 2
}

export enum CustomerStatus {
  Active = 0,
  Inactive = 1
}

export interface CustomerListItem {
  id: string;
  fullName: string;
  phoneNumber: string;
  email?: string;
  whatsAppNumber?: string;
  membershipTier: MembershipTier;
  /** Active subscription plan name, or "None" — the actual membership display value,
   *  derived server-side; membershipTier is a legacy field no longer manually edited. */
  membershipLabel: string;
  walletBalance: number;
  loyaltyPoints: number;
  status: CustomerStatus;
  createdAtUtc: string;
  lastOrderAtUtc?: string | null;
}

export interface UpdateMyProfileRequest {
  fullName: string;
  email?: string;
  whatsAppNumber?: string;
}

export interface CustomerAddress {
  id: string;
  label: string;
  line1: string;
  line2?: string;
  city: string;
  state: string;
  postalCode: string;
  isDefault: boolean;
}

export interface CustomerDetail {
  id: string;
  fullName: string;
  phoneNumber: string;
  email?: string;
  walletBalance: number;
  creditLimit: number;
  loyaltyPoints: number;
  membershipTier: MembershipTier;
  membershipLabel: string;
  status: CustomerStatus;
  notes?: string;
  addresses: CustomerAddress[];
}

export interface CreateCustomerAddressRequest {
  label: string;
  line1: string;
  line2?: string;
  city: string;
  state: string;
  postalCode: string;
  isDefault: boolean;
}

export interface CreateCustomerRequest {
  fullName: string;
  phoneNumber: string;
  email?: string;
  creditLimit: number;
  notes?: string;
  addresses?: CreateCustomerAddressRequest[];
}

export interface UpdateCustomerRequest {
  fullName: string;
  phoneNumber: string;
  email?: string;
  creditLimit: number;
  notes?: string;
}
