import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateReviewRequest, ProductReviewSummary, Review } from '../models/review.models';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5036/api/reviews';

  getByProductSlug(slug: string): Observable<ProductReviewSummary> {
    return this.http.get<ProductReviewSummary>(`${this.apiUrl}/product/${slug}`);
  }

  create(data: CreateReviewRequest): Observable<Review> {
    return this.http.post<Review>(this.apiUrl, data);
  }

  delete(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }
}
