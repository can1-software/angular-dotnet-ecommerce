import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.html',
})
export class AdminLayout implements OnInit {
  authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  pageTitle = signal('Admin Panel');

  ngOnInit(): void {
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => this.updateTitle());

    this.updateTitle();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  private updateTitle(): void {
    // Admin layout'un altındaki aktif child route'u bul.
    let child = this.route.firstChild;
    while (child?.firstChild) {
      child = child.firstChild;
    }

    const title = child?.snapshot?.data?.['title'];
    this.pageTitle.set(typeof title === 'string' ? title : 'Admin Panel');
  }
}
