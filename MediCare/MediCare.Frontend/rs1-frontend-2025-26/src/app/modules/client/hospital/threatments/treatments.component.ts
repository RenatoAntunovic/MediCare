import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import {
  ListTreatmentsRequest,
  ListTreatmentsQueryDto
} from '../../../../api-services/treatments/treatments-api.models';
import { TreatmentsApiService } from '../../../../api-services/treatments/treatments-api.service';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import {
  BookTreatmentComponent,
  BookTreatmentResult
} from '../../../client/book-treatment/book-treatment.component';

@Component({
  selector: 'app-treatments',
  standalone: false,
  templateUrl: './treatments.component.html',
  styleUrl: './treatments.component.scss'
})
export class TreatmentComponent
  extends BaseListPagedComponent<ListTreatmentsQueryDto, ListTreatmentsRequest>
  implements OnInit {

  private api = inject(TreatmentsApiService);
  private toaster = inject(ToasterService);
  private dialog = inject(MatDialog);

  displayedColumns: string[] = [
    'imageFile',
    'name',
    'treatmentsCategoryName',
    'price',
    'isEnabled',
    'actions'
  ];

  // Client-side "cooldown" (Namik's rate-limit task) – left as it was
  private lastRequestTime = 0;
  private requestCooldown = 2000;

  constructor() {
    super();
    this.request = new ListTreatmentsRequest();
  }

  ngOnInit(): void {
    this.initList();
  }

  protected loadPagedData(): void {
    const now = Date.now();
    if (now - this.lastRequestTime < this.requestCooldown) {
      this.toaster.error('Too many requests, please wait a few seconds.');
      return;
    }
    this.lastRequestTime = now;

    this.startLoading();

    this.api.list(this.request).subscribe({
      next: (response) => {
        this.items = response.items;
        this.stopLoading();
      },
      error: (err) => {
        console.error('Load error:', err);
        this.stopLoading('Failed to load treatments');
      }
    });
  }

  /**
   * Opens the booking calendar.
   * The dialog CREATES the reservation – here we only show a confirmation
   * (previously a second POST here would have created a duplicate reservation).
   */
  openBooking(treatment: ListTreatmentsQueryDto): void {
    if (!treatment.isEnabled) {
      this.toaster.error('Ovaj tretman trenutno nije dostupan za rezervaciju.');
      return;
    }

    this.dialog
      .open<BookTreatmentComponent, { treatment: ListTreatmentsQueryDto }, BookTreatmentResult>(
        BookTreatmentComponent,
        {
          width: '560px',
          maxWidth: '95vw',
          autoFocus: false,
          data: { treatment }
        }
      )
      .afterClosed()
      .subscribe(result => {
        if (!result) return;

        const [y, m, d] = result.date.split('-');
        this.toaster.success(
          `Rezervacija #${result.reservationId} kreirana: ${d}.${m}.${y}. u ${result.time}.`
        );
      });
  }

  onSearch(): void {
    this.request.paging.page = 1;
    this.loadPagedData();
  }
}
