export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  productImageUrl?: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  status: string;
  totalAmount: number;
  shippingFullName: string;
  shippingPhone: string;
  shippingAddress: string;
  shippingCity: string;
  createdAt: string;
  items: OrderItem[];
}

export interface CreateOrderRequest {
  shippingFullName: string;
  shippingPhone: string;
  shippingAddress: string;
  shippingCity: string;
}

export const ORDER_STATUS_LABELS: Record<string, string> = {
  Pending: 'Beklemede',
  Processing: 'Hazırlanıyor',
  Shipped: 'Kargoda',
  Delivered: 'Teslim Edildi',
  Cancelled: 'İptal Edildi',
};
