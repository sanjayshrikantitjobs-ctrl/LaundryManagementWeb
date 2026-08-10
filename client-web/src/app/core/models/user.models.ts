export enum UserRole {
  Admin = 0,
  StoreManager = 1,
  Staff = 2,
  DeliveryAgent = 3,
  Customer = 4,
  DepartmentHead = 5,
  PickupAgent = 6
}

export const STAFF_ROLES = [
  UserRole.Admin,
  UserRole.StoreManager,
  UserRole.Staff,
  UserRole.DepartmentHead,
  UserRole.PickupAgent,
  UserRole.DeliveryAgent
];

export interface UserSummary {
  id: string;
  userName: string;
  email?: string;
  phoneNumber?: string;
  fullName: string;
  role: UserRole;
  isActive: boolean;
}

export interface CreateUserRequest {
  fullName: string;
  userName: string;
  email?: string;
  phoneNumber?: string;
  password: string;
  role: UserRole;
}

export interface UpdateUserRequest {
  fullName: string;
  email?: string;
  phoneNumber?: string;
}
