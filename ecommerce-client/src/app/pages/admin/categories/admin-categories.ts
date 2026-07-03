import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CategoryService } from '../../../services/category.service';
import { Category } from '../../../models/category.models';

@Component({
  selector: 'app-admin-categories',
  imports: [RouterLink],
  templateUrl: './admin-categories.html',
})
export class AdminCategories implements OnInit {
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  categories = signal<Category[]>([]);
  loading = signal(true);
  errorMessage = signal('');
  successMessage = signal('');

  ngOnInit(): void {
    const msg = history.state?.['message'];
    if (msg) this.successMessage.set(msg);

    this.loadCategories();
  }

  loadCategories(): void {
    this.loading.set(true);
    this.categoryService.getAll().subscribe({
      next: (data) => {
        this.categories.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Kategoriler yüklenemedi.');
        this.loading.set(false);
      }
    });
  }

  deleteCategory(id: number, name: string): void {
    if (!confirm(`"${name}" kategorisini silmek istediğine emin misin?`)) return;

    this.errorMessage.set('');
    this.categoryService.delete(id).subscribe({
      next: () => {
        this.successMessage.set('Kategori silindi.');
        this.loadCategories();
      },
      error: (err) => this.errorMessage.set(err.error?.message ?? 'Silme başarısız.')
    });
  }
}
