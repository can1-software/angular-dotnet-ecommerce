import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateReviewRequest, ProductReviewSummary, Review } from '../models/review.models';
import { API_BASE_URL } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${API_BASE_URL}/api/reviews`;

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
