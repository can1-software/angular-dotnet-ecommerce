import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, UserRole } from '../models/auth.models';

// Bu servis, kimlik doğrulama (auth) ile ilgili her şeyi yönetir:
// - Backend'e register/login isteği atmak
// - Dönen token ve kullanıcı bilgisini tarayıcıda (localStorage) saklamak
// - "Şu an giriş yapılmış mı, kim yapmış?" bilgisini uygulamaya sunmak
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);

  // Backend API'nin adresi. (.NET uygulaması bu portta çalışıyor.)
  private readonly apiUrl = 'http://localhost:5036/api/auth';

  // localStorage'da veriyi saklarken kullanacağımız anahtar isimleri.
  private readonly tokenKey = 'token';
  private readonly userKey = 'user';

  // signal: değeri değiştiğinde onu kullanan her yer otomatik güncellenir.
  // Başlangıçta localStorage'dan mevcut kullanıcıyı okuruz (sayfa yenilense de giriş korunur).
  private currentUser = signal<AuthResponse | null>(this.readUserFromStorage());

  // Dışarıya salt-okunur bir görünüm sunuyoruz.
  user = this.currentUser.asReadonly();

  // Giriş yapılmış mı?
  isLoggedIn = computed(() => this.currentUser() !== null);

  // Admin mi?
  isAdmin = computed(() => this.currentUser()?.role === UserRole.Admin);

  // Kayıt ol: backend'e istek atar, başarılıysa kullanıcıyı oturum açmış sayar.
  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data).pipe(
      tap(response => this.saveSession(response))
    );
  }

  // Giriş yap: backend'e istek atar, başarılıysa oturumu kaydeder.
  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data).pipe(
      tap(response => this.saveSession(response))
    );
  }

  // Çıkış yap: saklanan bilgileri sil ve kullanıcıyı sıfırla.
  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUser.set(null);
  }

  // Başarılı giriş/kayıt sonrası oturumu tarayıcıya kaydeder.
  private saveSession(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.userKey, JSON.stringify(response));
    this.currentUser.set(response);
  }

  // Uygulama açıldığında localStorage'daki kullanıcıyı geri okur.
  private readUserFromStorage(): AuthResponse | null {
    const raw = localStorage.getItem(this.userKey);
    return raw ? (JSON.parse(raw) as AuthResponse) : null;
  }
}
