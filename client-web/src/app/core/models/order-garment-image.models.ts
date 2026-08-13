export enum GarmentImageType {
  Pickup = 0,
  Delivery = 1
}

export interface OrderGarmentImage {
  id: string;
  orderId: string;
  serviceId?: string | null;
  serviceName?: string | null;
  orderItemId?: string | null;
  imageType: GarmentImageType;
  imageUrl: string;
  uploadedByName: string;
  uploadedAtUtc: string;
  notes?: string | null;
}

export interface AddOrderGarmentImageRequest {
  imageType: GarmentImageType;
  imageUrl: string;
  serviceId?: string;
  orderItemId?: string;
  notes?: string;
}
