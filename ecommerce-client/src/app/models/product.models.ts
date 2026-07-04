export interface ProductImage {
  id: number;
  imageUrl: string;
  sortOrder: number;
}

export interface Product {
  id: number;
  name: string;
  slug: string;
  description?: string;
  metaTitle?: string;
  metaDescription?: string;
  metaKeywords?: string;
  price: number;
  stock: number;
  categoryId: number;
  categorySlug: string;
  categoryName: string;
  imageUrl?: string;
  images: ProductImage[];
  createdAt: string;
}

export interface CreateProductRequest {
  name: string;
  description?: string;
  metaTitle?: string;
  metaDescription?: string;
  metaKeywords?: string;
  price: number;
  stock: number;
  categoryId: number;
  image?: File;
  additionalImages?: File[];
}

export interface UpdateProductRequest {
  name: string;
  description?: string;
  metaTitle?: string;
  metaDescription?: string;
  metaKeywords?: string;
  price: number;
  stock: number;
  categoryId: number;
  image?: File;
  additionalImages?: File[];
}

export interface PagedProductResult {
  items: Product[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ProductQuery {
  search?: string;
  categoryId?: number | null;
  categorySlug?: string | null;
  page?: number;
  pageSize?: number;
}
