import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CategoryService } from '../../../services/category.service';
import { ProductService } from '../../../services/product.service';
import { Category } from '../../../models/category.models';

@Component({
  selector: 'app-admin-product-edit',
  imports: [FormsModule, RouterLink],
  templateUrl: './admin-product-edit.html',
})
export class AdminProductEdit implements OnInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  productId = 0;
  categories = signal<Category[]>([]);
  name = '';
  description = '';
  price: number | null = null;
  stock: number | null = null;
  categoryId: number | null = null;
  errorMessage = signal('');
  loading = signal(false);
  pageLoading = signal(true);

  ngOnInit(): void {
    this.productId = Number(this.route.snapshot.paramMap.get('id'));

    this.categoryService.getPaged({ page: 1, pageSize: 50 }).subscribe({
      next: (r) => this.categories.set(r.items),
    });

    this.productService.getById(this.productId).subscribe({
      next: (p) => {
        this.name = p.name;
        this.description = p.description ?? '';
        this.price = p.price;
        this.stock = p.stock;
        this.categoryId = p.categoryId;
        this.pageLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Ürün bulunamadı.');
        this.pageLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.categoryId === null || this.price === null || this.stock === null) return;

    this.errorMessage.set('');
    this.loading.set(true);

    this.productService.update(this.productId, {
      name: this.name,
      description: this.description || undefined,
      price: this.price,
      stock: this.stock,
      categoryId: this.categoryId,
    }).subscribe({
      next: () => {
        this.router.navigate(['/admin/products'], { state: { message: 'Ürün güncellendi.' } });
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message ?? 'Güncelleme başarısız.');
        this.loading.set(false);
      }
    });
  }
}
