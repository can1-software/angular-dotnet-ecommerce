import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
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
  private router = inject(Router);
  private searchTimeout?: ReturnType<typeof setTimeout>;

  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  loading = signal(true);
  errorMessage = signal('');

  searchTerm = '';
  selectedCategorySlug: string | null = null;
  currentPage = signal(1);
  pageSize = 12;
  totalCount = signal(0);
  totalPages = signal(0);

  resolveImageUrl = resolveImageUrl;

  ngOnInit(): void {
    this.loadCategories();

    this.route.paramMap.subscribe(params => {
      this.selectedCategorySlug = params.get('slug');
      this.currentPage.set(1);
      this.loadProducts();
    });
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

  selectCategory(slug: string | null): void {
    if (slug) {
      this.router.navigate(['/categories', slug]);
    } else {
      this.router.navigate(['/']);
    }
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
      categorySlug: this.selectedCategorySlug,
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
