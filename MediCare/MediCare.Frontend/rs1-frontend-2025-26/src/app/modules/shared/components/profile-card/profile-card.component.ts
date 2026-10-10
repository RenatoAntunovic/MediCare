import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';

import { MyProfileDto, ProfileApiService } from '../../../../api-services/profile/profile-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { startOfToday, toLocalDateString } from '../../../../core/utils/date-utils';

/**
 * "My profile" card on the Settings page: shows and edits the logged-in user's personal data.
 * Email and username are read-only (email is used to log in).
 */
@Component({
  selector: 'app-profile-card',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  providers: [
    provideNativeDateAdapter(),
    { provide: MAT_DATE_LOCALE, useValue: 'bs-BA' }
  ],
  templateUrl: './profile-card.component.html',
  styleUrl: './profile-card.component.scss'
})
export class ProfileCardComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(ProfileApiService);
  private toaster = inject(ToasterService);

  /** Read-only part of the profile (email, username) */
  profile: MyProfileDto | null = null;

  isLoading = true;
  isSaving = false;
  loadError = '';

  readonly maxBirthDate = startOfToday();

  // Same rules as UpdateMyProfileCommandValidator on the backend
  form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(50)]],
    lastName: ['', [Validators.required, Validators.maxLength(50)]],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^\+?[\d\s\-/]{9,20}$/)]],
    address: ['', [Validators.required, Validators.maxLength(100)]],
    city: ['', [Validators.required, Validators.maxLength(50)]],
    dateOfBirth: this.fb.control<Date | null>(null, Validators.required)
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.loadError = '';

    this.api.get().subscribe({
      next: profile => {
        this.fillForm(profile);
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Podaci profila se nisu mogli učitati.';
        this.isLoading = false;
      }
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    this.isSaving = true;

    this.api.update({
      firstName: v.firstName,
      lastName: v.lastName,
      phoneNumber: v.phoneNumber,
      address: v.address,
      city: v.city,
      dateOfBirth: toLocalDateString(v.dateOfBirth!) // local date, no UTC shift
    }).subscribe({
      next: profile => {
        this.fillForm(profile);
        this.isSaving = false;
        this.toaster.success('Profil je sačuvan.');
      },
      error: err => {
        this.isSaving = false;
        if (err.status === 429) return; // message already shown by the rate-limit interceptor
        this.toaster.error(err.error?.message || 'Profil nije sačuvan.');
      }
    });
  }

  /** Puts the saved values back (discards unsaved changes) */
  cancel(): void {
    if (this.profile) this.fillForm(this.profile);
  }

  private fillForm(profile: MyProfileDto): void {
    this.profile = profile;
    this.form.reset({
      firstName: profile.firstName,
      lastName: profile.lastName,
      phoneNumber: profile.phoneNumber,
      address: profile.address,
      city: profile.city,
      dateOfBirth: this.parseDate(profile.dateOfBirth)
    });
  }

  /** "2002-05-15T00:00:00" → local Date (without the timezone shift that new Date(string) can cause) */
  private parseDate(value: string): Date | null {
    if (!value) return null;
    const [y, m, d] = value.substring(0, 10).split('-').map(Number);
    return new Date(y, m - 1, d);
  }
}