import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DomSanitizer, Meta, Title } from '@angular/platform-browser';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';
import { ReviewService } from '../../services/review.service';
import { Product } from '../../models/product.models';
import { ProductReviewSummary } from '../../models/review.models';
import { FRONTEND_BASE_URL, resolveImageUrl } from '../../config/api.config';
import { extractApiErrorMessage } from '../../utils/api-error.util';

@Component({
  selector: 'app-product-detail',
  imports: [RouterLink, DecimalPipe, DatePipe, FormsModule],
  templateUrl: './product-detail.html',
})
export class ProductDetail implements OnInit {
  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private authService = inject(AuthService);
  private reviewService = inject(ReviewService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private sanitizer = inject(DomSanitizer);
  private title = inject(Title);
  private meta = inject(Meta);

  product = signal<Product | null>(null);
  loading = signal(true);
  errorMessage = signal('');
  cartMessage = signal('');
  addingToCart = signal(false);
  quantity = 1;
  selectedImageUrl = signal<string | null>(null);

  reviews = signal<ProductReviewSummary | null>(null);
  reviewsLoading = signal(false);
  reviewMessage = signal('');
  reviewSubmitting = signal(false);
  reviewRating = 5;
  reviewComment = '';
  deletingReviewId = signal<number | null>(null);

  isLoggedIn = this.authService.isLoggedIn;
  isAdmin = this.authService.isAdmin;

  hasOwnReview = computed(() =>
    (this.reviews()?.items ?? []).some(r => r.isOwnReview)
  );

  starOptions = [1, 2, 3, 4, 5];
  readonly Math = Math;

  galleryUrls = computed(() => {
    const p = this.product();
    if (!p) return [] as string[];

    const fromGallery = (p.images ?? [])
      .sort((a, b) => a.sortOrder - b.sortOrder)
      .map(i => resolveImageUrl(i.imageUrl))
      .filter((u): u is string => !!u);

    if (fromGallery.length) return fromGallery;

    const main = resolveImageUrl(p.imageUrl);
    return main ? [main] : [];
  });

  safeDescription = computed(() => {
    const html = this.product()?.description;
    if (!html) return null;
    return this.sanitizer.bypassSecurityTrustHtml(html);
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const slug = params.get('slug');
      if (!slug) return;

      this.loading.set(true);
      this.errorMessage.set('');

      this.productService.getBySlug(slug).subscribe({
        next: (p) => {
          this.product.set(p);
          const urls = this.buildGallery(p);
          this.selectedImageUrl.set(urls[0] ?? null);
          this.applySeo(p);
          this.loading.set(false);
          this.loadReviews(slug);
        },
        error: () => {
          this.errorMessage.set('Ürün bulunamadı.');
          this.loading.set(false);
        }
      });
    });
  }

  selectImage(url: string): void {
    this.selectedImageUrl.set(url);
  }

  loadReviews(slug: string): void {
    this.reviewsLoading.set(true);
    this.reviewService.getByProductSlug(slug).subscribe({
      next: (summary) => {
        this.reviews.set(summary);
        this.reviewsLoading.set(false);
      },
      error: () => {
        this.reviews.set({ averageRating: 0, totalCount: 0, items: [] });
        this.reviewsLoading.set(false);
      }
    });
  }

  submitReview(): void {
    const p = this.product();
    if (!p) return;

    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }

    if (this.reviewComment.trim().length < 5) {
      this.reviewMessage.set('Yorum en az 5 karakter olmalıdır.');
      return;
    }

    this.reviewSubmitting.set(true);
    this.reviewMessage.set('');

    this.reviewService.create({
      productId: p.id,
      rating: this.reviewRating,
      comment: this.reviewComment.trim()
    }).subscribe({
      next: () => {
        this.reviewComment = '';
        this.reviewRating = 5;
        this.reviewMessage.set('Yorumunuz eklendi!');
        this.reviewSubmitting.set(false);
        this.loadReviews(p.slug);
      },
      error: (err) => {
        this.reviewMessage.set(extractApiErrorMessage(err, 'Yorum eklenemedi.'));
        this.reviewSubmitting.set(false);
      }
    });
  }

  deleteReview(reviewId: number): void {
    const p = this.product();
    if (!p || !confirm('Bu yorumu silmek istediğinize emin misiniz?')) return;

    this.deletingReviewId.set(reviewId);
    this.reviewService.delete(reviewId).subscribe({
      next: () => {
        this.deletingReviewId.set(null);
        this.loadReviews(p.slug);
      },
      error: (err) => {
        this.reviewMessage.set(extractApiErrorMessage(err, 'Yorum silinemedi.'));
        this.deletingReviewId.set(null);
      }
    });
  }

  starsArray(rating: number): boolean[] {
    return [1, 2, 3, 4, 5].map(n => n <= rating);
  }

  addToCart(): void {
    const p = this.product();
    if (!p || p.stock < 1) return;

    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }

    this.addingToCart.set(true);
    this.cartMessage.set('');

    this.cartService.addItem({ productId: p.id, quantity: this.quantity }).subscribe({
      next: () => {
        this.cartMessage.set('Ürün sepete eklendi!');
        this.addingToCart.set(false);
      },
      error: (err) => {
        this.cartMessage.set(err.error?.message ?? 'Sepete eklenemedi.');
        this.addingToCart.set(false);
      }
    });
  }

  private buildGallery(p: Product): string[] {
    const fromGallery = (p.images ?? [])
      .sort((a, b) => a.sortOrder - b.sortOrder)
      .map(i => resolveImageUrl(i.imageUrl))
      .filter((u): u is string => !!u);

    if (fromGallery.length) return fromGallery;

    const main = resolveImageUrl(p.imageUrl);
    return main ? [main] : [];
  }

  private applySeo(p: Product): void {
    const pageTitle = p.metaTitle || `${p.name} | NovaShop`;
    const description = p.metaDescription || p.name;
    const image = resolveImageUrl(p.imageUrl) ?? '';
    const url = `${FRONTEND_BASE_URL}/products/${p.slug}`;

    this.title.setTitle(pageTitle);
    this.meta.updateTag({ name: 'description', content: description });
    if (p.metaKeywords) {
      this.meta.updateTag({ name: 'keywords', content: p.metaKeywords });
    }

    this.meta.updateTag({ property: 'og:title', content: pageTitle });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:type', content: 'product' });
    if (image) this.meta.updateTag({ property: 'og:image', content: image });
    this.meta.updateTag({ property: 'og:url', content: url });

    this.meta.updateTag({ name: 'twitter:card', content: 'summary_large_image' });
    this.meta.updateTag({ name: 'twitter:title', content: pageTitle });
    this.meta.updateTag({ name: 'twitter:description', content: description });
    if (image) this.meta.updateTag({ name: 'twitter:image', content: image });
  }
}
