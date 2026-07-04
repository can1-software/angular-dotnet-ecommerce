import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DomSanitizer, Meta, Title } from '@angular/platform-browser';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product.models';
import { resolveImageUrl } from '../../config/api.config';

@Component({
  selector: 'app-product-detail',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './product-detail.html',
})
export class ProductDetail implements OnInit {
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private sanitizer = inject(DomSanitizer);
  private title = inject(Title);
  private meta = inject(Meta);

  product = signal<Product | null>(null);
  loading = signal(true);
  errorMessage = signal('');
  selectedImageUrl = signal<string | null>(null);

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
    this.title.setTitle(p.metaTitle || `${p.name} | NovaShop`);
    this.meta.updateTag({ name: 'description', content: p.metaDescription || p.name });
    if (p.metaKeywords) {
      this.meta.updateTag({ name: 'keywords', content: p.metaKeywords });
    }
  }
}
