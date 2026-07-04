import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateProductRequest,
  PagedProductResult,
  Product,
  ProductQuery,
  UpdateProductRequest
} from '../models/product.models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5036/api/products';

  getPaged(query: ProductQuery = {}): Observable<PagedProductResult> {
    let params = new HttpParams()
      .set('page', String(query.page ?? 1))
      .set('pageSize', String(query.pageSize ?? 10));

    if (query.search?.trim()) {
      params = params.set('search', query.search.trim());
    }

    if (query.categoryId) {
      params = params.set('categoryId', String(query.categoryId));
    }

    return this.http.get<PagedProductResult>(this.apiUrl, { params });
  }

  getById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  create(data: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, this.toFormData(data));
  }

  update(id: number, data: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, this.toFormData(data));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private toFormData(data: CreateProductRequest | UpdateProductRequest): FormData {
    const formData = new FormData();
    formData.append('name', data.name);
    if (data.description) {
      formData.append('description', data.description);
    }
    formData.append('price', String(data.price));
    formData.append('stock', String(data.stock));
    formData.append('categoryId', String(data.categoryId));
    if (data.image) {
      formData.append('image', data.image);
    }
    return formData;
  }
}
