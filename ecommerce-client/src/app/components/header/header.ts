import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

// Sayfanın üstünde her zaman görünen menü çubuğu.
// Giriş durumuna göre farklı butonlar gösterir.
@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.html',
})
export class Header {
  // AuthService'i içeri alıyoruz; giriş durumu ve kullanıcı bilgisi buradan gelir.
  authService = inject(AuthService);
  private router = inject(Router);

  // Çıkış butonuna basılınca çalışır.
  logout(): void {
    this.authService.logout();
    // Çıkış sonrası ana sayfaya yönlendir.
    this.router.navigate(['/']);
  }
}
