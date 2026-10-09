import { Injectable, effect, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { initializeApp } from 'firebase/app';
import { Messaging, getMessaging, getToken, isSupported, onMessage } from 'firebase/messaging';
import { environment } from '../../../environments/environment';
import { AuthFacadeService } from './auth/auth-facade.service';
import { ToasterService } from './toaster.service';

/**
 * Push notifications (Firebase Cloud Messaging).
 * When a user is logged in it asks for notification permission, gets the FCM token
 * for this browser and registers it on the backend. Foreground messages are shown as a toast;
 * background messages are shown by the service worker (public/firebase-messaging-sw.js).
 */
@Injectable({ providedIn: 'root' })
export class PushNotificationService {
  private http = inject(HttpClient);
  private auth = inject(AuthFacadeService);
  private toaster = inject(ToasterService);

  private messaging: Messaging | null = null;
  private registeredForUserId: number | null = null;

  constructor() {
    // React to login / logout (and to a page refresh while logged in)
    effect(() => {
      const user = this.auth.currentUser();

      if (!user) {
        this.registeredForUserId = null;
        return;
      }

      if (user.userId !== this.registeredForUserId) {
        this.registeredForUserId = user.userId;
        void this.register();
      }
    });
  }

  private async register(): Promise<void> {
    try {
      // Some browsers (or private windows) don't support web push
      if (!(await isSupported())) return;

      // The user already said "Block" – never ask again
      if (Notification.permission === 'denied') return;

      const permission = Notification.permission === 'granted'
        ? 'granted'
        : await Notification.requestPermission();
      if (permission !== 'granted') return;

      if (!this.messaging) {
        this.messaging = getMessaging(initializeApp(environment.firebase));

        // Message arrived while the app is open → show it inside the app
        onMessage(this.messaging, payload => {
          const title = payload.notification?.title ?? 'MediCare';
          const body = payload.notification?.body ?? '';
          this.toaster.info(`${title}: ${body}`);
        });
      }

      // Register the worker and wait until it is ACTIVE – right after register() it may still be installing
      await navigator.serviceWorker.register('/firebase-messaging-sw.js');
      const registration = await navigator.serviceWorker.ready;

      const token = await getToken(this.messaging, {
        vapidKey: environment.firebaseVapidKey,
        serviceWorkerRegistration: registration
      });
      if (!token) return;

      this.http.post(`${environment.apiUrl}/api/Notifications/token`, { token }).subscribe({
        error: err => console.warn('Could not save the FCM token on the server:', err)
      });
    } catch (err) {
      // Notifications are optional – never break the app because of them
      console.warn('Push notifications could not be enabled:', err);
    }
  }
}