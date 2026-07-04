import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { extractApiErrorMessage } from '../../utils/api-error.util';

// Kayıt sayfası (adres: "/register").
@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
})
export class Register {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Formdaki alanlar.
  fullName = '';
  email = '';
  password = '';

  errorMessage = signal('');
  loading = signal(false);

  onSubmit(): void {
    this.errorMessage.set('');
    this.loading.set(true);

    this.authService
      .register({ fullName: this.fullName, email: this.email, password: this.password })
      .subscribe({
        // Kayıt başarılı: otomatik giriş yapılmış olur, ana sayfaya git.
        next: () => this.router.navigate(['/']),
        error: (err) => {
          this.errorMessage.set(extractApiErrorMessage(err, 'Kayıt yapılamadı.'));
          this.loading.set(false);
        }
      });
  }
}
