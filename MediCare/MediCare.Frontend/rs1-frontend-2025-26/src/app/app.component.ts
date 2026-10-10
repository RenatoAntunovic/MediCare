import { Component, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ThemeService } from './core/services/theme.service';
import { PushNotificationService } from './core/services/push-notification.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  protected readonly title = signal('rs1-frontend-2025-26');
  currentLang = 'bs';

  /** created here so the saved light/dark choice is applied on every page from the first render */
  protected readonly theme = inject(ThemeService);
  /** created at startup so push notifications are registered as soon as a user is logged in */
  private readonly push = inject(PushNotificationService);
  private readonly translate = inject(TranslateService);

  constructor() {
    this.translate.addLangs(['en', 'bs']);
    this.translate.setDefaultLang('bs');

    // Load the language from localStorage or use the default
    const savedLang = localStorage.getItem('language') || 'bs';
    this.currentLang = savedLang;

    this.translate.use(savedLang).subscribe({
      error: (error) => console.error('Error loading translations:', error)
    });
  }

  switchLanguage(lang: string): void {
    this.currentLang = lang;
    localStorage.setItem('language', lang);
    this.translate.use(lang).subscribe({
      error: (error) => console.error('Error switching language:', error)
    });
  }
}