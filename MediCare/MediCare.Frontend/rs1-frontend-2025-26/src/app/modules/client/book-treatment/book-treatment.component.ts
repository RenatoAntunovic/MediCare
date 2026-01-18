import { Component, Inject } from '@angular/core';
import { CommonModule, registerLocaleData } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormGroup
} from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import localeBs from '@angular/common/locales/bs';

interface TreatmentData {
  id: number;
  serviceName: string;
  price: number;
}

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
    MatNativeDateModule,
    MatIconModule,
    MatSelectModule,
    MatSnackBarModule
  ],
  templateUrl: './book-treatment.component.html',
  styleUrls: ['./book-treatment.component.scss']
})
export class BookTreatmentComponent {
  bookingForm!: FormGroup;
  timeSlots: string[] = [];

  constructor(
    public dialogRef: MatDialogRef<BookTreatmentComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { treatment: TreatmentData },
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private http: HttpClient  // ✅ Backend poziv
  ) {
    registerLocaleData(localeBs);
    this.generateTimeSlots();
    
    this.bookingForm = this.fb.group({
      date: ['', Validators.required],
      time: ['', Validators.required],
      description: ['']
    });
  }

  generateTimeSlots(): void {
    for (let h = 8; h <= 17; h++) {
      this.timeSlots.push(`${h.toString().padStart(2, '0')}:00`);
      if (h < 17) this.timeSlots.push(`${h.toString().padStart(2, '0')}:30`);
    }
  }

  dateFilter = (date: Date | null): boolean => {
    if (!date) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const day = date.getDay();
    return (day !== 0 && day !== 6) && date > today;
  };

  dateClass = (date: Date): string => {
    return date.getDay() >= 1 && date.getDay() <= 5 ? 'available-date' : 'mat-calendar-body-weekend';
  };

  saveBooking(): void {
  if (this.bookingForm.invalid) return;

  const date = this.bookingForm.value.date;
  const [hours, minutes] = this.bookingForm.value.time.split(':').map(Number);

  // ✅ DateTime ISO string - C# parsira automatski
  const fullDateTime = new Date(date.getFullYear(), date.getMonth(), date.getDate(), hours, minutes);
  
  const command = {
    treatmentId: this.data.treatment.id,
    reservationDate: fullDateTime.toISOString(),  // "2027-04-22T09:30:00.000Z"
    reservationTime: this.bookingForm.value.time, // "09:30"
    userId: 1  // ili iz auth servisa
  };

  console.log('🟢 FIXED:', command);

  this.http.post<{ reservationId: number }>('api/Reservations', command).subscribe({
    next: res => {
      console.log('✅', res);
      this.snackBar.open(`#${res.reservationId} OK!`, 'OK');
      this.dialogRef.close();
    },
    error: err => {
      console.error('❌', err);
      this.snackBar.open(err.error?.error || 'Greška', 'Zatvori');
    }
  });
}

}




