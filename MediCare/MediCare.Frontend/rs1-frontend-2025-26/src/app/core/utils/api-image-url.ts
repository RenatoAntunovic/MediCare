import { environment } from '../../../environments/environment';

/**
 * Full URL of an image served by the API (wwwroot/images).
 * Works for paths with or without a leading slash ("images/a.png" and "/images/a.png").
 */
export function apiImageUrl(path: string | null | undefined): string {
  if (!path) return '';
  return `${environment.apiUrl}/${path.replace(/^\/+/, '')}`;
}