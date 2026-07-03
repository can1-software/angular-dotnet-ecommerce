import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CategoryService } from '../../../services/category.service';
import { ProductService } from '../../../services/product.service';
import { Category } from '../../../models/category.models';

@Component({
  selector: 'app-admin-product-create',
  imports: [FormsModule, RouterLink],
  templateUrl: './admin-product-create.html',
})
export class AdminProductCreate implements OnInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  categories = signal<Category[]>([]);
  name = '';
  description = '';
  price: number | null = null;
  stock: number | null = null;
  categoryId: number | null = null;
  errorMessage = signal('');
  loading = signal(false);

  ngOnInit(): void {
    this.categoryService.getPaged({ page: 1, pageSize: 50 }).subscribe({
      next: (r) => this.categories.set(r.items),
      error: () => this.errorMessage.set('Kategoriler yüklenemedi.')
    });
  }

  onSubmit(): void {
    if (this.categoryId === null || this.price === null || this.stock === null) return;

    this.errorMessage.set('');
    this.loading.set(true);

    this.productService.create({
      name: this.name,
      description: this.description || undefined,
      price: this.price,
      stock: this.stock,
      categoryId: this.categoryId,
    }).subscribe({
      next: () => {
        this.router.navigate(['/admin/products'], { state: { message: 'Ürün başarıyla eklendi.' } });
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message ?? 'Ürün eklenemedi.');
        this.loading.set(false);
      }
    });
  }
}
