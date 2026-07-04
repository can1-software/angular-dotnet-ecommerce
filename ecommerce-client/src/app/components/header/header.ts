import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.html',
})
export class Header implements OnInit {
  authService = inject(AuthService);
  cartService = inject(CartService);
  private router = inject(Router);

  readonly githubUrl = 'https://github.com/can1-software/angular-dotnet-ecommerce';

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.cartService.refreshCount();
    }
  }

  logout(): void {
    this.authService.logout();
    this.cartService.clearCount();
    this.router.navigate(['/']);
  }
}
