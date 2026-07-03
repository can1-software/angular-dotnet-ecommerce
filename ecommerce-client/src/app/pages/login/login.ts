import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

// Giriş sayfası (adres: "/login").
@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Formdaki input'lara bağlanacak alanlar (two-way binding ile [(ngModel)]).
  email = '';
  password = '';

  // Kullanıcıya gösterilecek hata mesajı ve "işlem sürüyor" durumu.
  errorMessage = signal('');
  loading = signal(false);

  // Form gönderildiğinde çalışır.
  onSubmit(): void {
    this.errorMessage.set('');
    this.loading.set(true);

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      // Başarılı: ana sayfaya yönlendir.
      next: () => this.router.navigate(['/']),
      // Hata: backend'den gelen mesajı göster.
      error: (err) => {
        this.errorMessage.set(err.error?.message ?? 'Giriş yapılamadı. Bilgileri kontrol et.');
        this.loading.set(false);
      }
    });
  }
}
