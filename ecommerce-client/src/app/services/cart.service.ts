import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AddToCartRequest, Cart, UpdateCartItemRequest } from '../models/cart.models';
import { API_BASE_URL } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export class CartService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${API_BASE_URL}/api/cart`;

  cartItemCount = signal(0);

  getCart(): Observable<Cart> {
    return this.http.get<Cart>(this.apiUrl).pipe(
      tap(cart => this.cartItemCount.set(cart.totalItems))
    );
  }

  addItem(data: AddToCartRequest): Observable<Cart> {
    return this.http.post<Cart>(`${this.apiUrl}/items`, data).pipe(
      tap(cart => this.cartItemCount.set(cart.totalItems))
    );
  }

  updateItem(cartItemId: number, data: UpdateCartItemRequest): Observable<Cart> {
    return this.http.put<Cart>(`${this.apiUrl}/items/${cartItemId}`, data).pipe(
      tap(cart => this.cartItemCount.set(cart.totalItems))
    );
  }

  removeItem(cartItemId: number): Observable<Cart> {
    return this.http.delete<Cart>(`${this.apiUrl}/items/${cartItemId}`).pipe(
      tap(cart => this.cartItemCount.set(cart.totalItems))
    );
  }

  refreshCount(): void {
    this.getCart().subscribe({ error: () => this.cartItemCount.set(0) });
  }

  clearCount(): void {
    this.cartItemCount.set(0);
  }
}
