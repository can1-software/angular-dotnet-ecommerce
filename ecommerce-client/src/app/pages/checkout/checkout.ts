import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { AuthService } from '../../services/auth.service';
import { Cart } from '../../models/cart.models';
import { extractApiErrorMessage } from '../../utils/api-error.util';

@Component({
  selector: 'app-checkout',
  imports: [RouterLink, FormsModule, DecimalPipe],
  templateUrl: './checkout.html',
})
export class Checkout implements OnInit {
  private cartService = inject(CartService);
  private orderService = inject(OrderService);
  private authService = inject(AuthService);
  private router = inject(Router);

  cart = signal<Cart | null>(null);
  loading = signal(true);
  submitting = signal(false);
  errorMessage = signal('');

  shippingFullName = '';
  shippingPhone = '';
  shippingAddress = '';
  shippingCity = '';

  ngOnInit(): void {
    const user = this.authService.user();
    if (user) this.shippingFullName = user.fullName;

    this.cartService.getCart().subscribe({
      next: (c) => {
        this.cart.set(c);
        this.loading.set(false);
        if (!c.items.length) this.router.navigate(['/cart']);
      },
      error: () => {
        this.errorMessage.set('Sepet yüklenemedi.');
        this.loading.set(false);
      }
    });
  }

  onSubmit(): void {
    this.errorMessage.set('');
    this.submitting.set(true);

    this.orderService.create({
      shippingFullName: this.shippingFullName,
      shippingPhone: this.shippingPhone,
      shippingAddress: this.shippingAddress,
      shippingCity: this.shippingCity,
    }).subscribe({
      next: (order) => {
        this.cartService.clearCount();
        this.router.navigate(['/orders', order.id], { state: { message: 'Siparişiniz başarıyla oluşturuldu!' } });
      },
      error: (err) => {
        this.errorMessage.set(extractApiErrorMessage(err, 'Sipariş oluşturulamadı.'));
        this.submitting.set(false);
      }
    });
  }
}
