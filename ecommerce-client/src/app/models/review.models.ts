export interface Review {
  id: number;
  productId: number;
  userName: string;
  rating: number;
  comment: string;
  createdAt: string;
  isOwnReview: boolean;
}

export interface ProductReviewSummary {
  averageRating: number;
  totalCount: number;
  items: Review[];
}

export interface CreateReviewRequest {
  productId: number;
  rating: number;
  comment: string;
}
