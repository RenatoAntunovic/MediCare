import { Component, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ThemeService } from '../../../core/services/theme.service';
import { AuthFacadeService } from '../../../core/services/auth/auth-facade.service';

/**
 * Settings page: shows the signed-in account and the two preferences the app already has
 * (light/dark theme and language). Both use the same services as the sidebar, so they always stay in sync.
 */
@Component({
  selector: 'app-admin-settings',
  standalone: false,
  templateUrl: './admin-settings.component.html',
  styleUrl: './admin-settings.component.scss',
})
export class AdminSettingsComponent {
  private translate = inject(TranslateService);
  auth = inject(AuthFacadeService);
  theme = inject(ThemeService);

  currentLang: string = this.translate.currentLang || 'bs';

  languages = [
    { code: 'bs', name: 'Bosanski', flag: '🇧🇦' },
    { code: 'en', name: 'English', flag: '🇬🇧' },
  ];

  switchLanguage(langCode: string): void {
    this.currentLang = langCode;
    this.translate.use(langCode);
    localStorage.setItem('language', langCode);
  }
}
