export interface ActivePromotion {
  id: string;
  title: string;
  description?: string;
  imageUrl?: string;
  code?: string;
  discountPercent?: number;
  discountAmount?: number;
  validTo?: string | null;
}

export interface PromotionListItem {
  id: string;
  title: string;
  description?: string;
  imageUrl?: string;
  code?: string;
  discountPercent?: number;
  discountAmount?: number;
  validFrom?: string | null;
  validTo?: string | null;
  isActive: boolean;
}

export interface SavePromotionRequest {
  title: string;
  description?: string;
  imageUrl?: string;
  code?: string;
  discountPercent?: number | null;
  discountAmount?: number | null;
  validFrom?: string | null;
  validTo?: string | null;
  isActive: boolean;
}
