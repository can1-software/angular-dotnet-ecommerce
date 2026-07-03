import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../../services/product.service';
import { Product } from '../../../models/product.models';

@Component({
  selector: 'app-admin-products',
  imports: [RouterLink, FormsModule, DecimalPipe],
  templateUrl: './admin-products.html',
})
export class AdminProducts implements OnInit {
  private productService = inject(ProductService);
  private searchTimeout?: ReturnType<typeof setTimeout>;

  products = signal<Product[]>([]);
  loading = signal(true);
  errorMessage = signal('');
  successMessage = signal('');

  searchTerm = '';
  currentPage = signal(1);
  pageSize = signal(10);
  totalCount = signal(0);
  totalPages = signal(0);

  ngOnInit(): void {
    const msg = history.state?.['message'];
    if (msg) this.successMessage.set(msg);
    this.loadProducts();
  }

  onSearchChange(): void {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage.set(1);
      this.loadProducts();
    }, 400);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.currentPage()) return;
    this.currentPage.set(page);
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.productService.getPaged({
      search: this.searchTerm,
      page: this.currentPage(),
      pageSize: this.pageSize(),
    }).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.currentPage.set(result.page);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Ürünler yüklenemedi.');
        this.loading.set(false);
      }
    });
  }

  deleteProduct(id: number, name: string): void {
    if (!confirm(`"${name}" ürününü silmek istediğine emin misin?`)) return;

    this.errorMessage.set('');
    this.productService.delete(id).subscribe({
      next: () => {
        this.successMessage.set('Ürün silindi.');
        if (this.products().length === 1 && this.currentPage() > 1) {
          this.currentPage.update(p => p - 1);
        }
        this.loadProducts();
      },
      error: (err) => this.errorMessage.set(err.error?.message ?? 'Silme başarısız.')
    });
  }
}
