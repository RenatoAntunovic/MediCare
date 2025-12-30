import { Component, OnInit, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { environment } from '../environments/environment';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  protected readonly title = signal('rs1-frontend-2025-26');
  currentLang: string = 'bs';

  constructor(
    private translate: TranslateService, 
    private snackBar: MatSnackBar) {
    console.log('AppComponent constructor - initializing TranslateService');

    // Inicijalizacija translate servisa
    this.translate.addLangs(['en', 'bs']);
    this.translate.setDefaultLang('bs');

    // Učitaj jezik iz localStorage ili koristi default
    const savedLang = localStorage.getItem('language') || 'bs';
    this.currentLang = savedLang;

    this.translate.use(savedLang).subscribe({
      next: (translations) => {
        console.log('Translations loaded successfully for language:', savedLang);
        console.log('Available keys:', Object.keys(translations));
      },
      error: (error) => {
        console.error('Error loading translations:', error);
        console.error('Check if files exist at: /i18n/' + savedLang + '.json');

        
      }
    });
  }

  ngOnInit(): void {
    // Test translation
    this.translate.get('MEDICINE.TITLE').subscribe((res: string) => {
      console.log('Translation for MEDICINE.TITLE:', res);
      if (res === 'MEDICINE.TITLE') {
        console.error('⚠️ Translation not working! Key returned instead of value.');
        console.error('Possible causes:');
        console.error('1. Translation files not in /i18n/ folder');
        console.error('2. JSON files have syntax errors');
        console.error('3. TranslateService not properly initialized');
      }
    });

    
}

  // app.component.ts



  switchLanguage(lang: string): void {
    this.currentLang = lang;
    localStorage.setItem('language', lang);
    this.translate.use(lang).subscribe({
      next: () => {
        console.log('Language switched to:', lang);
      },
      error: (error) => {
        console.error('Error switching language:', error);
      }
    });
  }
}
