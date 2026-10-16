import { Component } from '@angular/core';

/** Admin AI assistant page – read-only questions about bookings, orders, stock and statistics. */
@Component({
  selector: 'app-admin-assistant',
  standalone: false,
  template: `
    <div class="mc-page">
      <app-assistant-chat mode="admin" [examples]="examples"></app-assistant-chat>
    </div>
  `,
})
export class AdminAssistantComponent {
  readonly examples = [
    'ASSISTANT.EXAMPLES.ADMIN_TODAY',
    'ASSISTANT.EXAMPLES.ADMIN_ORDERS',
    'ASSISTANT.EXAMPLES.ADMIN_STOCK',
    'ASSISTANT.EXAMPLES.ADMIN_MONTH',
  ];
}
