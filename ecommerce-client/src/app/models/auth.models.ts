// Backend'e gönderdiğimiz ve backend'den aldığımız verilerin tipleri.
// TypeScript'te böyle "interface" tanımlamak, yanlış alan kullanımını önler.

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// Backend'in register/login sonrası döndürdüğü cevap.
export interface AuthResponse {
  fullName: string;
  email: string;
  token: string;
}
