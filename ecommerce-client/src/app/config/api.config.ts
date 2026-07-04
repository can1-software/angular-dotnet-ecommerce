import { environment } from '../../environments/environment';

export const API_BASE_URL = environment.apiUrl;
export const FRONTEND_BASE_URL = environment.frontendUrl || (typeof window !== 'undefined' ? window.location.origin : '');

export function resolveImageUrl(imageUrl?: string | null): string | null {
  if (!imageUrl) return null;
  if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
    return imageUrl;
  }
  return `${API_BASE_URL}${imageUrl.startsWith('/') ? '' : '/'}${imageUrl}`;
}
