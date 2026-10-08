import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';

import { OrdersApiService } from '../../../../api-services/orders/orders-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { startOfToday, toLocalDateString } from '../../../../core/utils/date-utils';
import { downloadBlob, readBlobErrorMessage } from '../../../../core/utils/download-file';

type Preset = 'thisMonth' | 'lastMonth' | 'last30' | 'thisYear' | 'all';

@Component({
  selector: 'app-orders-report-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatIconModule
  ],
  providers: [
    provideNativeDateAdapter(),
    { provide: MAT_DATE_LOCALE, useValue: 'bs-BA' }
  ],
  templateUrl: './orders-report-dialog.component.html',
  styleUrl: './orders-report-dialog.component.scss'
})
export class OrdersReportDialogComponent {
  private fb = inject(FormBuilder);
  private ordersApi = inject(OrdersApiService);
  private toaster = inject(ToasterService);
  private dialogRef = inject(MatDialogRef<OrdersReportDialogComponent>);

  /** Must match the OrderStatus table (seed: 1–5). */
  readonly statuses = [
    { id: 1, label: 'Nacrt' },
    { id: 2, label: 'Potvrđena' },
    { id: 3, label: 'Plaćena' },
    { id: 4, label: 'Završena' },
    { id: 5, label: 'Otkazana' }
  ];

  readonly presets: { key: Preset; label: string }[] = [
    { key: 'thisMonth', label: 'Ovaj mjesec' },
    { key: 'lastMonth', label: 'Prošli mjesec' },
    { key: 'last30', label: 'Zadnjih 30 dana' },
    { key: 'thisYear', label: 'Ova godina' },
    { key: 'all', label: 'Sve' }
  ];

  activePreset: Preset | null = 'thisMonth';
  isGenerating = false;
  errorMessage = '';

  form = this.fb.group({
    range: this.fb.group({
      start: this.fb.control<Date | null>(null),
      end: this.fb.control<Date | null>(null)
    }),
    statusId: this.fb.control<number | null>(null)
  });

  private applyingPreset = false;

  constructor() {
    this.applyPreset('thisMonth');

    // Manually changing the dates clears the quick-pick highlight
    this.form.controls.range.valueChanges.subscribe(() => {
      if (!this.applyingPreset) this.activePreset = null;
    });
  }

  applyPreset(preset: Preset): void {
    const today = startOfToday();
    let start: Date | null = null;
    let end: Date | null = null;

    switch (preset) {
      case 'thisMonth':
        start = new Date(today.getFullYear(), today.getMonth(), 1);
        end = today;
        break;
      case 'lastMonth':
        start = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        end = new Date(today.getFullYear(), today.getMonth(), 0); // last day of the previous month
        break;
      case 'last30':
        start = new Date(today);
        start.setDate(start.getDate() - 29);
        end = today;
        break;
      case 'thisYear':
        start = new Date(today.getFullYear(), 0, 1);
        end = today;
        break;
      case 'all':
        break;
    }

    this.applyingPreset = true;
    this.form.controls.range.setValue({ start, end });
    this.applyingPreset = false;
    this.activePreset = preset;
    this.errorMessage = '';
  }

  get periodSummary(): string {
    const { start, end } = this.form.controls.range.getRawValue();
    const fmt = (d: Date) => d.toLocaleDateString('bs-BA');
    if (!start && !end) return 'sve narudžbe';
    if (start && !end) return `od ${fmt(start)}`;
    if (!start && end) return `do ${fmt(end)}`;
    return `${fmt(start!)} – ${fmt(end!)}`;
  }

  generate(): void {
    if (this.isGenerating) return;

    const { range, statusId } = this.form.getRawValue();
    const from = range.start ? toLocalDateString(range.start) : null;
    const to = range.end ? toLocalDateString(range.end) : null;

    this.isGenerating = true;
    this.errorMessage = '';

    this.ordersApi.downloadReportPdf({ from, to, statusId }).subscribe({
      next: blob => {
        this.isGenerating = false;
        const suffix = from || to ? `${from ?? 'pocetak'}_${to ?? 'danas'}` : 'sve';
        downloadBlob(blob, `Izvjestaj_narudzbi_${suffix}.pdf`);
        this.toaster.success('Izvještaj je preuzet.');
        this.dialogRef.close(true);
      },
      error: async err => {
        this.errorMessage = await readBlobErrorMessage(err, 'Generisanje izvještaja nije uspjelo.');
        this.isGenerating = false;
      }
    });
  }
}
