export enum PricingType {
  PerItem = 0,
  WeightBased = 1
}

export interface ServiceCategory {
  id: string;
  name: string;
  description?: string;
  iconOrImageUrl?: string;
  displayOrder: number;
  isActive: boolean;
}

export interface CreateServiceCategoryRequest {
  name: string;
  description?: string;
  iconOrImageUrl?: string;
  displayOrder: number;
}

export interface UpdateServiceCategoryRequest extends CreateServiceCategoryRequest {
  isActive: boolean;
}

export interface AddOn {
  id: string;
  name: string;
  description?: string;
  price: number;
  isActive: boolean;
}

export interface CreateAddOnRequest {
  name: string;
  description?: string;
  price: number;
}

export interface UpdateAddOnRequest extends CreateAddOnRequest {
  isActive: boolean;
}

export interface GarmentListItem {
  id: string;
  name: string;
  categoryId: string;
  categoryName: string;
  imageUrl?: string;
}

export interface GarmentServicePriceItem {
  serviceId: string;
  serviceName: string;
  pricingType: PricingType;
  price: number;
}

export interface GarmentDetail {
  id: string;
  name: string;
  categoryId: string;
  categoryName: string;
  specialInstructions?: string;
  imageUrl?: string;
  servicePrices: GarmentServicePriceItem[];
}

export interface CreateGarmentRequest {
  name: string;
  categoryId: string;
  specialInstructions?: string;
  imageUrl?: string;
}

export type UpdateGarmentRequest = CreateGarmentRequest;

export interface ServiceListItem {
  id: string;
  name: string;
  categoryId: string;
  categoryName: string;
  basePrice: number;
  estimatedTimeHours: number;
  gstPercentage: number;
  priority: number;
  imageUrl?: string;
  expressSurcharge: number;
  expressEtaHours: number;
}

export interface ServiceDetail {
  id: string;
  name: string;
  categoryId: string;
  categoryName: string;
  basePrice: number;
  estimatedTimeHours: number;
  gstPercentage: number;
  priority: number;
  imageUrl?: string;
  expressSurcharge: number;
  expressEtaHours: number;
}

export interface CreateServiceRequest {
  name: string;
  categoryId: string;
  basePrice: number;
  estimatedTimeHours: number;
  gstPercentage: number;
  priority: number;
  imageUrl?: string;
  expressSurcharge: number;
  expressEtaHours: number;
}

export type UpdateServiceRequest = CreateServiceRequest;

export interface PricingMatrixService {
  id: string;
  name: string;
  categoryId: string;
  categoryName: string;
}

export interface PricingMatrixCell {
  serviceId: string;
  pricingType: PricingType | null;
  price: number | null;
  expressPrice: number | null;
  gstPercentage: number | null;
  estimatedTimeHours: number | null;
  expressEtaHours: number | null;
  isActive: boolean;
}

export interface PricingMatrixGarmentRow {
  garmentId: string;
  garmentName: string;
  categoryId: string;
  categoryName: string;
  prices: PricingMatrixCell[];
}

export interface PricingMatrix {
  services: PricingMatrixService[];
  garments: PricingMatrixGarmentRow[];
}

export interface SetGarmentServicePriceRequest {
  pricingType: PricingType;
  price: number;
  expressPrice?: number | null;
  gstPercentage?: number | null;
  estimatedTimeHours?: number | null;
  expressEtaHours?: number | null;
  isActive: boolean;
}
