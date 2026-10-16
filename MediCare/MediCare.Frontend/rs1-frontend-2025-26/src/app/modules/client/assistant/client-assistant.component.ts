import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { CartsApiService } from '../../../api-services/carts/carts-api.service';
import { ToasterService } from '../../../core/services/toaster.service';
import {
  AssistantMedicineSuggestion,
  AssistantSlotSuggestion,
  AssistantTreatmentSuggestion,
} from '../../../api-services/assistant/assistant-api.models';
import {
  BookTreatmentComponent,
  BookTreatmentPreselect,
  BookTreatmentResult,
} from '../book-treatment/book-treatment.component';

/**
 * Patient AI assistant page. The assistant only suggests –
 * the patient confirms: booking goes through the normal calendar dialog, cart through the normal cart API.
 */
@Component({
  selector: 'app-client-assistant',
  standalone: false,
  template: `
    <div class="mc-page">
      <app-assistant-chat
        mode="patient"
        [examples]="examples"
        (addToCart)="onAddToCart($event)"
        (openMedicine)="onOpenMedicine($event)"
        (bookSlot)="onBookSlot($event)"
        (bookTreatment)="onBookTreatment($event)"
      ></app-assistant-chat>
    </div>
  `,
})
export class ClientAssistantComponent {
  private cartApi = inject(CartsApiService);
  private toaster = inject(ToasterService);
  private dialog = inject(MatDialog);
  private router = inject(Router);
  private translate = inject(TranslateService);

  readonly examples = [
    'ASSISTANT.EXAMPLES.PATIENT_HEADACHE',
    'ASSISTANT.EXAMPLES.PATIENT_BACK',
    'ASSISTANT.EXAMPLES.PATIENT_SLOTS',
    'ASSISTANT.EXAMPLES.PATIENT_MY_BOOKINGS',
  ];

  onAddToCart(medicine: AssistantMedicineSuggestion): void {
    this.cartApi.addToCart({ medicineId: medicine.id, quantity: 1 }).subscribe({
      next: () => this.toaster.success(this.translate.instant('ASSISTANT.ADDED_TO_CART', { name: medicine.name })),
      error: () => this.toaster.error(this.translate.instant('ASSISTANT.ERRORS.CART')),
    });
  }

  onOpenMedicine(medicine: AssistantMedicineSuggestion): void {
    this.router.navigate(['/client/medicine', medicine.id]);
  }

  onBookSlot(slot: AssistantSlotSuggestion): void {
    this.openBooking(
      { id: slot.treatmentId, serviceName: slot.treatmentName, price: slot.price },
      { date: slot.date, time: slot.time }
    );
  }

  onBookTreatment(treatment: AssistantTreatmentSuggestion): void {
    this.openBooking({ id: treatment.id, serviceName: treatment.name, price: treatment.price });
  }

  /** Same dialog as on the Treatments page – the dialog itself creates the reservation. */
  private openBooking(
    treatment: { id: number; serviceName: string; price: number },
    preselect?: BookTreatmentPreselect
  ): void {
    this.dialog
      .open<BookTreatmentComponent, unknown, BookTreatmentResult>(BookTreatmentComponent, {
        width: '560px',
        maxWidth: '95vw',
        autoFocus: false,
        data: { treatment, preselect },
      })
      .afterClosed()
      .subscribe(result => {
        if (!result) return;
        const [y, m, d] = result.date.split('-');
        this.toaster.success(
          `Rezervacija #${result.reservationId} kreirana: ${d}.${m}.${y}. u ${result.time}.`
        );
      });
  }
}
