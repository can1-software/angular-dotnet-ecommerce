import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

// Ana sayfa (adres: "/").
@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.html',
})
export class Home {
  // Giriş durumuna göre farklı metin göstermek için servisi alıyoruz.
  authService = inject(AuthService);
}
