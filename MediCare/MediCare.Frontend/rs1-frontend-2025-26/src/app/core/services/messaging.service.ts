import { Injectable, inject } from '@angular/core';
import { FirebaseApp} from '@angular/fire/app';
import { initializeApp } from 'firebase/app';
import { getMessaging, getToken, onMessage, Messaging } from 'firebase/messaging';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MessagingService {
  private app: FirebaseApp;
  private messaging: Messaging;

  constructor() {
    // Modularno inicijaliziraj app (jednom)
    this.app = initializeApp(environment.firebase);
    this.messaging = getMessaging(this.app);
  }

  async requestPermission(vapidKey: string): Promise<string> {
    const permission = await Notification.requestPermission();
    if (permission !== 'granted') throw new Error('Permission not granted');

    const token = await getToken(this.messaging, { vapidKey });
    return token;
  }

  receiveMessage(callback: (payload: any) => void) {
    onMessage(this.messaging, callback);
  }
}
