import { HttpErrorResponse } from '@angular/common/http';

/**
 * Triggers a Blob download in the browser.
 */
export function downloadBlob(blob: Blob, fileName: string): void {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  // Small delay so Firefox can start the download before the URL is revoked
  setTimeout(() => window.URL.revokeObjectURL(url), 1000);
}

/**
 * For requests with responseType: 'blob', the error also arrives as a Blob,
 * so the backend message (ErrorDto.message) has to be read manually.
 */
export async function readBlobErrorMessage(err: unknown, fallback: string): Promise<string> {
  if (err instanceof HttpErrorResponse && err.error instanceof Blob) {
    try {
      const text = await err.error.text();
      const json = JSON.parse(text);
      return json?.message || fallback;
    } catch {
      return fallback;
    }
  }

  if (err instanceof HttpErrorResponse && err.error?.message) {
    return err.error.message;
  }

  return fallback;
}
