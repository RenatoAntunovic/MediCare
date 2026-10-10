import { Injectable, effect, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { initializeApp } from 'firebase/app';
import { Messaging, deleteToken, getMessaging, getToken, isSupported, onMessage } from 'firebase/messaging';
import { environment } from '../../../environments/environment';
import { AuthFacadeService } from './auth/auth-facade.service';
import { ToasterService } from './toaster.service';

/** State of push notifications on THIS device/browser */
export type PushStatus = 'checking' | 'unsupported' | 'blocked' | 'on' | 'off';

/** Remembers on this browser that the user turned notifications off in Settings */
const OFF_KEY = 'medicare.push-off';

/**
 * Push notifications (Firebase Cloud Messaging).
 * When a user is logged in it asks for notification permission, gets the FCM token
 * for this browser and registers it on the backend. Foreground messages are shown as a toast;
 * background messages are shown by the service worker (public/firebase-messaging-sw.js).
 * The user can turn notifications on/off on the Settings page.
 */
@Injectable({ providedIn: 'root' })
export class PushNotificationService {
  private http = inject(HttpClient);
  private auth = inject(AuthFacadeService);
  private toaster = inject(ToasterService);

  private messaging: Messaging | null = null;
  private registeredForUserId: number | null = null;

  /** Read by the Settings card */
  readonly status = signal<PushStatus>('checking');

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

  /** Settings → switch ON: forget the "off" choice and register again (asks for permission if needed) */
  async enable(): Promise<void> {
    this.setTurnedOff(false);
    await this.register();
  }

  /** Settings → switch OFF: the server forgets the token and this browser won't register again on login */
  async disable(): Promise<void> {
    this.setTurnedOff(true);
    this.status.set('off');

    this.http.delete(`${environment.apiUrl}/api/Notifications/token`).subscribe({
      error: err => console.warn('Could not remove the FCM token on the server:', err)
    });

    try {
      if (this.messaging) await deleteToken(this.messaging);
    } catch {
      // not important – the server no longer has the token anyway
    }
  }

  /** Sends a test notification to the current user (demo button on the Settings page) */
  sendTest(): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/api/Notifications/test`, {});
  }

  private async register(): Promise<void> {
    try {
      // Some browsers (or private windows) don't support web push
      if (!(await isSupported())) {
        this.status.set('unsupported');
        return;
      }

      // The user clicked "Block" in the browser – we can't ask again
      if (Notification.permission === 'denied') {
        this.status.set('blocked');
        return;
      }

      // Turned off in Settings on this browser
      if (this.isTurnedOff()) {
        this.status.set('off');
        return;
      }

      const permission = Notification.permission === 'granted'
        ? 'granted'
        : await Notification.requestPermission();

      if (permission !== 'granted') {
        this.status.set(permission === 'denied' ? 'blocked' : 'off');
        return;
      }

      const messaging = this.getMessagingInstance();

      // Register the worker and wait until it is ACTIVE – right after register() it may still be installing
      await navigator.serviceWorker.register('/firebase-messaging-sw.js');
      const registration = await navigator.serviceWorker.ready;

      const token = await getToken(messaging, {
        vapidKey: environment.firebaseVapidKey,
        serviceWorkerRegistration: registration
      });
      if (!token) {
        this.status.set('off');
        return;
      }

      this.http.post(`${environment.apiUrl}/api/Notifications/token`, { token }).subscribe({
        next: () => this.status.set('on'),
        error: err => {
          console.warn('Could not save the FCM token on the server:', err);
          this.status.set('off');
        }
      });
    } catch (err) {
      // Notifications are optional – never break the app because of them
      console.warn('Push notifications could not be enabled:', err);
      this.status.set('off');
    }
  }

  /** Firebase is initialised only once */
  private getMessagingInstance(): Messaging {
    if (!this.messaging) {
      this.messaging = getMessaging(initializeApp(environment.firebase));

      // Message arrived while the app is open → show it inside the app
      onMessage(this.messaging, payload => {
        const title = payload.notification?.title ?? 'MediCare';
        const body = payload.notification?.body ?? '';
        this.toaster.info(`${title}: ${body}`);
      });
    }
    return this.messaging;
  }

  // localStorage can throw (private mode, blocked storage) – never let that break the app
  private isTurnedOff(): boolean {
    try { return localStorage.getItem(OFF_KEY) === '1'; } catch { return false; }
  }

  private setTurnedOff(off: boolean): void {
    try {
      if (off) localStorage.setItem(OFF_KEY, '1');
      else localStorage.removeItem(OFF_KEY);
    } catch { /* ignore */ }
  }
}