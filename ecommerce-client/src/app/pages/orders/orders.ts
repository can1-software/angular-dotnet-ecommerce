import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { Order, ORDER_STATUS_LABELS } from '../../models/order.models';

@Component({
  selector: 'app-orders',
  imports: [RouterLink, DecimalPipe, DatePipe],
  templateUrl: './orders.html',
})
export class Orders implements OnInit {
  private orderService = inject(OrderService);

  orders = signal<Order[]>([]);
  loading = signal(true);
  errorMessage = signal('');

  statusLabel = (status: string) => ORDER_STATUS_LABELS[status] ?? status;

  ngOnInit(): void {
    this.orderService.getMyOrders().subscribe({
      next: (o) => {
        this.orders.set(o);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Siparişler yüklenemedi.');
        this.loading.set(false);
      }
    });
  }
}
