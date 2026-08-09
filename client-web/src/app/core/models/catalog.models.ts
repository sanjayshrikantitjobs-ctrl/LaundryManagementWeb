export enum PricingType {
  PerItem = 0,
  WeightBased = 1
}

export interface GarmentListItem {
  id: string;
  name: string;
  category: string;
  barcode?: string;
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
  category: string;
  barcode?: string;
  specialInstructions?: string;
  imageUrl?: string;
  servicePrices: GarmentServicePriceItem[];
}

export interface CreateGarmentRequest {
  name: string;
  category: string;
  barcode?: string;
  specialInstructions?: string;
  imageUrl?: string;
}

export type UpdateGarmentRequest = CreateGarmentRequest;

export interface ServiceListItem {
  id: string;
  name: string;
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
}

export interface PricingMatrixCell {
  serviceId: string;
  pricingType: PricingType | null;
  price: number | null;
}

export interface PricingMatrixGarmentRow {
  garmentId: string;
  garmentName: string;
  category: string;
  prices: PricingMatrixCell[];
}

export interface PricingMatrix {
  services: PricingMatrixService[];
  garments: PricingMatrixGarmentRow[];
}
