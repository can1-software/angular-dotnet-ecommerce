import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { Cart, CartItem } from '../../models/cart.models';
import { resolveImageUrl } from '../../config/api.config';

@Component({
  selector: 'app-cart',
  imports: [RouterLink, FormsModule, DecimalPipe],
  templateUrl: './cart.html',
})
export class CartPage implements OnInit {
  private cartService = inject(CartService);
  private router = inject(Router);

  cart = signal<Cart | null>(null);
  loading = signal(true);
  errorMessage = signal('');
  successMessage = signal('');

  resolveImageUrl = resolveImageUrl;

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.loading.set(true);
    this.cartService.getCart().subscribe({
      next: (c) => {
        this.cart.set(c);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Sepet yüklenemedi.');
        this.loading.set(false);
      }
    });
  }

  updateQuantity(item: CartItem, quantity: number): void {
    if (quantity < 1 || quantity > item.stock) return;
    this.cartService.updateItem(item.id, { quantity }).subscribe({
      next: (c) => this.cart.set(c),
      error: (err) => this.errorMessage.set(err.error?.message ?? 'Güncelleme başarısız.')
    });
  }

  removeItem(item: CartItem): void {
    this.cartService.removeItem(item.id).subscribe({
      next: (c) => {
        this.cart.set(c);
        this.successMessage.set('Ürün sepetten çıkarıldı.');
      },
      error: (err) => this.errorMessage.set(err.error?.message ?? 'Silme başarısız.')
    });
  }

  goToCheckout(): void {
    if (!this.cart()?.items.length) return;
    this.router.navigate(['/checkout']);
  }
}
