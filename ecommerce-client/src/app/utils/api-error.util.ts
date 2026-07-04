import { HttpErrorResponse } from '@angular/common/http';

/** API'den dönen hata gövdesini okunabilir Türkçe mesaja çevirir. */
export function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  const body = error.error;

  if (!body) {
    if (error.status === 0) {
      return 'Sunucuya bağlanılamadı. API çalışıyor mu?';
    }
    if (error.status === 401) {
      return 'Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın.';
    }
    return `${fallback} (HTTP ${error.status})`;
  }

  if (typeof body === 'string') {
    return body.trim() || fallback;
  }

  if (typeof body.message === 'string' && body.message.trim()) {
    return body.message.trim();
  }

  if (body.errors && typeof body.errors === 'object') {
    const messages = Object.entries(body.errors as Record<string, string[]>)
      .flatMap(([field, msgs]) =>
        (msgs ?? []).map(msg => {
          const label = fieldLabel(field);
          return label ? `${label}: ${msg}` : msg;
        })
      )
      .filter(Boolean);

    if (messages.length) {
      return messages.join(' ');
    }
  }

  if (typeof body.title === 'string' && body.title.trim()) {
    return body.title.trim();
  }

  return `${fallback} (HTTP ${error.status})`;
}

function fieldLabel(field: string): string {
  const labels: Record<string, string> = {
    shippingFullName: 'Ad Soyad',
    shippingPhone: 'Telefon',
    shippingAddress: 'Adres',
    shippingCity: 'Şehir',
    email: 'E-posta',
    password: 'Şifre',
    fullName: 'Ad Soyad',
  };
  return labels[field] ?? '';
}
