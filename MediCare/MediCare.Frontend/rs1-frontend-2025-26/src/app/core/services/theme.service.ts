import { Injectable, effect, signal } from '@angular/core';

/**
 * Single owner of the light / dark theme for the whole app (public, auth, client and admin).
 *
 * - The choice is stored in localStorage under "darkMode" ("true" / "false") – the same key the old
 *   per-page toggle used, so a user's existing preference keeps working.
 * - Dark mode is applied by adding the `dark-mode` class to <body>; the design tokens do the rest.
 * - Default is light, as before.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'darkMode';

  /** true while dark mode is on – read it in templates as theme.isDark() */
  readonly isDark = signal<boolean>(this.readSaved());

  constructor() {
    effect(() => {
      document.body.classList.toggle('dark-mode', this.isDark());
    });
  }

  toggle(): void {
    this.set(!this.isDark());
  }

  set(dark: boolean): void {
    this.isDark.set(dark);
    try {
      localStorage.setItem(this.storageKey, dark ? 'true' : 'false');
    } catch {
      // storage can be blocked (private mode); the theme still works for this session
    }
  }

  private readSaved(): boolean {
    try {
      return localStorage.getItem(this.storageKey) === 'true';
    } catch {
      return false;
    }
  }
}
