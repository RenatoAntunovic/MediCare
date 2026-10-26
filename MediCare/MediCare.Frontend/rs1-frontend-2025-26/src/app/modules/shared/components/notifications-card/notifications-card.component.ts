import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { PushNotificationService } from '../../../../core/services/push-notification.service';
import { ToasterService } from '../../../../core/services/toaster.service';

/**
 * "Notifications" card on the Settings page: turn push notifications on/off on this device
 * and send a test notification.
 */
@Component({
  selector: 'app-notifications-card',
  standalone: true,
  imports: [CommonModule, MatSlideToggleModule, MatButtonModule, MatIconModule],
  templateUrl: './notifications-card.component.html',
  styleUrl: './notifications-card.component.scss'
})
export class NotificationsCardComponent {
  private push = inject(PushNotificationService);
  private toaster = inject(ToasterService);

  readonly status = this.push.status;
  readonly isOn = computed(() => this.status() === 'on');
  /** The switch can't be used when the browser doesn't support push or the user blocked it */
  readonly isLocked = computed(() => ['checking', 'unsupported', 'blocked'].includes(this.status()));

  isBusy = false;
  isSendingTest = false;

  async onToggle(checked: boolean): Promise<void> {
    this.isBusy = true;
    if (checked) {
      await this.push.enable();
      if (this.push.status() === 'blocked') {
        this.toaster.error('Obavještenja su blokirana u pregledniku.');
      }
    } else {
      await this.push.disable();
      this.toaster.success('Obavještenja su isključena na ovom uređaju.');
    }
    this.isBusy = false;
  }

  sendTest(): void {
    this.isSendingTest = true;
    this.push.sendTest().subscribe({
      next: () => {
        this.isSendingTest = false;
        this.toaster.success('Test obavještenje je poslano.');
      },
      error: err => {
        this.isSendingTest = false;
        if (err.status === 429) return; // message already shown by the rate-limit interceptor
        this.toaster.error('Test obavještenje nije poslano.');
      }
    });
  }
}