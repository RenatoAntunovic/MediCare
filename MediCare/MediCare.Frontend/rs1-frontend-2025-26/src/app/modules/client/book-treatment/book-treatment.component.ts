import { Component, DestroyRef, Inject, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subscription } from 'rxjs';

import { ReservationsService } from '../../../api-services/reservations/reservations.service';
import { startOfToday, toLocalDateString } from '../../../core/utils/date-utils';

interface TreatmentData {
  id: number;
  serviceName: string;
  price: number;
}

export interface BookTreatmentResult {
  reservationId: number;
  date: string;
  time: string;
}

/** Must match ReservationRules on the backend (08:00–17:00, every 30 min). */
const FIRST_HOUR = 8;
const LAST_HOUR = 17;
const NOTES_MAX_LENGTH = 1000;

@Component({
  selector: 'app-book-treatment',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  providers: [
    provideNativeDateAdapter(),
    { provide: MAT_DATE_LOCALE, useValue: 'bs-BA' } // Monday as first day, format dd.MM.yyyy
  ],
  templateUrl: './book-treatment.component.html',
  styleUrls: ['./book-treatment.component.scss']
})
export class BookTreatmentComponent {
  private fb = inject(FormBuilder);
  private reservations = inject(ReservationsService);
  private destroyRef = inject(DestroyRef);

  readonly notesMaxLength = NOTES_MAX_LENGTH;
  readonly timeSlots: string[] = this.generateTimeSlots();
  readonly minDate = this.tomorrow();

  bookingForm = this.fb.group({
    date: this.fb.control<Date | null>(null, Validators.required),
    time: this.fb.control<string | null>(null, Validators.required),
    description: this.fb.control<string>('', Validators.maxLength(NOTES_MAX_LENGTH))
  });

  takenSlots = new Set<string>();
  isLoadingSlots = false;
  slotsError = '';
  isSaving = false;
  errorMessage = '';

  private slotsSubscription?: Subscription;

  constructor(
    public dialogRef: MatDialogRef<BookTreatmentComponent, BookTreatmentResult>,
    @Inject(MAT_DIALOG_DATA) public data: { treatment: TreatmentData }
  ) {
    // On every date change, load the taken slots for that day
    this.bookingForm.controls.date.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(date => {
        this.bookingForm.controls.time.reset(null);
        this.errorMessage = '';
        if (date) this.loadTakenSlots(date);
        else this.takenSlots.clear();
      });
  }

  // ==================== CALENDAR ====================

  /** Only working days are allowed, tomorrow at the earliest. */
  dateFilter = (date: Date | null): boolean => {
    if (!date) return false;
    const day = date.getDay();
    return day !== 0 && day !== 6 && date >= this.minDate;
  };

  /** Colors working days in the calendar (style is in the .scss). */
  dateClass = (date: Date): string =>
    this.dateFilter(date) ? 'available-date' : '';

  get selectedDate(): Date | null {
    return this.bookingForm.controls.date.value;
  }

  /** e.g. "utorak, 20. 10. 2026." – via Intl, no need for registerLocaleData */
  get selectedDateLabel(): string {
    const d = this.selectedDate;
    if (!d) return '';
    return d.toLocaleDateString('bs-BA', { weekday: 'long', day: 'numeric', month: 'numeric', year: 'numeric' });
  }

  get freeSlotsCount(): number {
    return this.timeSlots.filter(s => !this.takenSlots.has(s)).length;
  }

  isTaken(slot: string): boolean {
    return this.takenSlots.has(slot);
  }

  isSelected(slot: string): boolean {
    return this.bookingForm.controls.time.value === slot;
  }

  selectSlot(slot: string): void {
    if (this.isTaken(slot) || this.isLoadingSlots) return;
    this.bookingForm.controls.time.setValue(slot);
    this.bookingForm.controls.time.markAsTouched();
    this.errorMessage = '';
  }

  private loadTakenSlots(date: Date): void {
    this.slotsSubscription?.unsubscribe(); // cancel the previous request if the user changes the date quickly
    this.isLoadingSlots = true;
    this.slotsError = '';
    this.takenSlots.clear();

    this.slotsSubscription = this.reservations
      .getTakenSlots(this.data.treatment.id, toLocalDateString(date))
      .subscribe({
        next: slots => {
          this.takenSlots = new Set(slots);
          this.isLoadingSlots = false;
        },
        error: () => {
          this.isLoadingSlots = false;
          this.slotsError = 'Nije moguće učitati zauzete termine. Možete pokušati rezervisati – sistem će provjeriti dostupnost.';
        }
      });
  }

  // ==================== SAVING ====================

  saveBooking(): void {
    if (this.bookingForm.invalid || this.isSaving) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    const { date, time, description } = this.bookingForm.getRawValue();
    if (!date || !time) return;

    const reservationDate = toLocalDateString(date);   // "2026-10-20" – no UTC shift
    const reservationTime = `${time}:00`;              // "09:30:00" – a format TimeSpan parses reliably

    this.isSaving = true;
    this.errorMessage = '';

    this.reservations.createReservation({
      treatmentId: this.data.treatment.id,
      reservationDate,
      reservationTime,
      notes: description?.trim() || null
    }).subscribe({
      next: res => {
        this.isSaving = false;
        this.dialogRef.close({ reservationId: res.reservationId, date: reservationDate, time });
      },
      error: (err: HttpErrorResponse) => {
        this.isSaving = false;
        this.errorMessage = err.error?.message || 'Rezervacija nije uspjela. Pokušajte ponovo.';

        // 409 = someone else took the slot in the meantime → refresh the slots
        if (err.status === 409 && date) {
          this.bookingForm.controls.time.reset(null);
          this.loadTakenSlots(date);
        }
      }
    });
  }

  // ==================== HELPERS ====================

  private generateTimeSlots(): string[] {
    const slots: string[] = [];
    for (let h = FIRST_HOUR; h <= LAST_HOUR; h++) {
      const hh = h.toString().padStart(2, '0');
      slots.push(`${hh}:00`);
      if (h < LAST_HOUR) slots.push(`${hh}:30`);
    }
    return slots;
  }

  private tomorrow(): Date {
    const d = startOfToday();
    d.setDate(d.getDate() + 1);
    return d;
  }
}
