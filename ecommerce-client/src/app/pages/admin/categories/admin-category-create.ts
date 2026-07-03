import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CategoryService } from '../../../services/category.service';

@Component({
  selector: 'app-admin-category-create',
  imports: [FormsModule, RouterLink],
  templateUrl: './admin-category-create.html',
})
export class AdminCategoryCreate {
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  name = '';
  description = '';
  errorMessage = signal('');
  loading = signal(false);

  onSubmit(): void {
    this.errorMessage.set('');
    this.loading.set(true);

    this.categoryService
      .create({ name: this.name, description: this.description || undefined })
      .subscribe({
        next: () => {
          this.router.navigate(['/admin/categories'], {
            state: { message: 'Kategori başarıyla eklendi.' }
          });
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message ?? 'Kategori eklenemedi.');
          this.loading.set(false);
        }
      });
  }
}
