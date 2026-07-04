import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { Order, ORDER_STATUS_LABELS } from '../../models/order.models';
import { resolveImageUrl } from '../../config/api.config';

@Component({
  selector: 'app-order-detail',
  imports: [RouterLink, DecimalPipe, DatePipe],
  templateUrl: './order-detail.html',
})
export class OrderDetail implements OnInit {
  private orderService = inject(OrderService);
  private route = inject(ActivatedRoute);

  order = signal<Order | null>(null);
  loading = signal(true);
  errorMessage = signal('');
  successMessage = signal(history.state?.['message'] ?? '');

  resolveImageUrl = resolveImageUrl;
  statusLabel = (status: string) => ORDER_STATUS_LABELS[status] ?? status;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.orderService.getById(id).subscribe({
      next: (o) => {
        this.order.set(o);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Sipariş bulunamadı.');
        this.loading.set(false);
      }
    });
  }
}
