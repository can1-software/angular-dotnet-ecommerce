import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { QuillModule } from 'ngx-quill';
import { CategoryService } from '../../../services/category.service';
import { ProductService } from '../../../services/product.service';
import { Category } from '../../../models/category.models';

@Component({
  selector: 'app-admin-product-create',
  imports: [FormsModule, RouterLink, QuillModule],
  templateUrl: './admin-product-create.html',
})
export class AdminProductCreate implements OnInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  categories = signal<Category[]>([]);
  name = '';
  description = '';
  metaTitle = '';
  metaDescription = '';
  metaKeywords = '';
  price: number | null = null;
  stock: number | null = null;
  categoryId: number | null = null;
  selectedImage: File | null = null;
  additionalImages: File[] = [];
  imagePreview = signal<string | null>(null);
  additionalPreviews = signal<string[]>([]);
  errorMessage = signal('');
  loading = signal(false);

  editorModules = {
    toolbar: [
      ['bold', 'italic', 'underline'],
      [{ header: [2, 3, false] }],
      [{ list: 'ordered' }, { list: 'bullet' }],
      ['link'],
      ['clean']
    ]
  };

  ngOnInit(): void {
    this.categoryService.getPaged({ page: 1, pageSize: 50 }).subscribe({
      next: (r) => this.categories.set(r.items),
      error: () => this.errorMessage.set('Kategoriler yüklenemedi.')
    });
  }

  onImageSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.selectedImage = file;
    const reader = new FileReader();
    reader.onload = () => this.imagePreview.set(reader.result as string);
    reader.readAsDataURL(file);
  }

  onAdditionalImagesSelected(event: Event): void {
    const files = Array.from((event.target as HTMLInputElement).files ?? []);
    this.additionalImages = files;
    this.additionalPreviews.set([]);
    files.forEach(file => {
      const reader = new FileReader();
      reader.onload = () => this.additionalPreviews.update(p => [...p, reader.result as string]);
      reader.readAsDataURL(file);
    });
  }

  clearImage(): void {
    this.selectedImage = null;
    this.imagePreview.set(null);
  }

  onSubmit(): void {
    if (this.categoryId === null || this.price === null || this.stock === null) return;

    this.errorMessage.set('');
    this.loading.set(true);

    this.productService.create({
      name: this.name,
      description: this.description || undefined,
      metaTitle: this.metaTitle || undefined,
      metaDescription: this.metaDescription || undefined,
      metaKeywords: this.metaKeywords || undefined,
      price: this.price,
      stock: this.stock,
      categoryId: this.categoryId,
      image: this.selectedImage ?? undefined,
      additionalImages: this.additionalImages.length ? this.additionalImages : undefined,
    }).subscribe({
      next: () => this.router.navigate(['/admin/products'], { state: { message: 'Ürün başarıyla eklendi.' } }),
      error: (err) => {
        this.errorMessage.set(err.error?.message ?? 'Ürün eklenemedi.');
        this.loading.set(false);
      }
    });
  }
}
