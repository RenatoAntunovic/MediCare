import { Component, OnInit, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MessagingService } from './core/services/messaging.service';
import { environment } from '../environments/environment';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  protected readonly title = signal('rs1-frontend-2025-26');
  currentLang: string = 'bs';

  constructor(
    private translate: TranslateService, 
    private messagingService:MessagingService,
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

     this.messagingService.receiveMessage(payload => {
    console.log('New FCM message received:', payload);
    if (payload.notification) {
      this.snackBar.open(
        `${payload.notification.title}: ${payload.notification.body}`,
        'OK',
        { duration: 5000 }
      );
    }
  });
}

  // app.component.ts
requestNotifications() {
  this.messagingService.requestPermission(environment.firebase.vapidKey)
    .then(token => {
      console.log('FCM Token:', token);
      this.snackBar.open('Push notifikacije omogućene!', 'OK', { duration: 3000 });
    })
    .catch(err => {
      console.error('Permission denied for notifications', err);
      this.snackBar.open('Dozvola odbijena!', 'OK', { duration: 3000 });
    });
}


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
