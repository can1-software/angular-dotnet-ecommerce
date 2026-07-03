import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CategoryService } from '../../../services/category.service';

@Component({
  selector: 'app-admin-category-edit',
  imports: [FormsModule, RouterLink],
  templateUrl: './admin-category-edit.html',
})
export class AdminCategoryEdit implements OnInit {
  private categoryService = inject(CategoryService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  categoryId = 0;
  name = '';
  description = '';
  errorMessage = signal('');
  loading = signal(false);
  pageLoading = signal(true);

  ngOnInit(): void {
    this.categoryId = Number(this.route.snapshot.paramMap.get('id'));

    this.categoryService.getById(this.categoryId).subscribe({
      next: (cat) => {
        this.name = cat.name;
        this.description = cat.description ?? '';
        this.pageLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Kategori bulunamadı.');
        this.pageLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    this.errorMessage.set('');
    this.loading.set(true);

    this.categoryService
      .update(this.categoryId, { name: this.name, description: this.description || undefined })
      .subscribe({
        next: () => {
          this.router.navigate(['/admin/categories'], {
            state: { message: 'Kategori güncellendi.' }
          });
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message ?? 'Güncelleme başarısız.');
          this.loading.set(false);
        }
      });
  }
}
