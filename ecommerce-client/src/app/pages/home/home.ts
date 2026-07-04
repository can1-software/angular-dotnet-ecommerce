import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { Product } from '../../models/product.models';
import { Category } from '../../models/category.models';
import { resolveImageUrl } from '../../config/api.config';

@Component({
  selector: 'app-home',
  imports: [RouterLink, FormsModule, DecimalPipe],
  templateUrl: './home.html',
})
export class Home implements OnInit {
  authService = inject(AuthService);
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private route = inject(ActivatedRoute);
  private searchTimeout?: ReturnType<typeof setTimeout>;

  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  loading = signal(true);
  errorMessage = signal('');

  searchTerm = '';
  selectedCategoryId: number | null = null;
  currentPage = signal(1);
  pageSize = 12;
  totalCount = signal(0);
  totalPages = signal(0);

  resolveImageUrl = resolveImageUrl;

  ngOnInit(): void {
    const categoryId = this.route.snapshot.queryParamMap.get('categoryId');
    if (categoryId) {
      this.selectedCategoryId = Number(categoryId);
    }
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.categoryService.getPaged({ page: 1, pageSize: 50 }).subscribe({
      next: (r) => this.categories.set(r.items),
    });
  }

  onSearchChange(): void {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage.set(1);
      this.loadProducts();
    }, 400);
  }

  selectCategory(categoryId: number | null): void {
    this.selectedCategoryId = categoryId;
    this.currentPage.set(1);
    this.loadProducts();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.currentPage()) return;
    this.currentPage.set(page);
    this.loadProducts();
    document.getElementById('products')?.scrollIntoView({ behavior: 'smooth' });
  }

  loadProducts(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.productService.getPaged({
      search: this.searchTerm,
      categoryId: this.selectedCategoryId,
      page: this.currentPage(),
      pageSize: this.pageSize,
    }).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.currentPage.set(result.page);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Ürünler yüklenemedi. API çalışıyor mu?');
        this.loading.set(false);
      }
    });
  }
}
