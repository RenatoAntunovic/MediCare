import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Subject, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, finalize, map, switchMap, tap } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

import { MedicineApiService } from '../../../api-services/medicine/medicine-api.service';
import { environment } from '../../../../environments/environment';

/** A single result from GET /api/search */
export interface MedicineSearchItem {
  id: number;
  name: string;
  description: string;
  price: number;
  category: string;
  imagePath: string;
  weight: number;
}

interface MedicineSearchResponse {
  query: string;
  page: number;
  pageSize: number;
  total: number;
  results: MedicineSearchItem[];
}

const MIN_QUERY_LENGTH = 2;

@Component({
  standalone: true,
  selector: 'app-search-medicine',
  templateUrl: './search-medicine.component.html',
  styleUrls: ['./search-medicine.component.scss'],
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule]
})
export class SearchMedicineComponent implements OnInit {
  private medicineApi = inject(MedicineApiService);
  private destroyRef = inject(DestroyRef);

  /** Every input goes through this stream – debounce + switchMap solve the race condition. */
  private search$ = new Subject<string>();

  searchQuery = '';
  medicines: MedicineSearchItem[] = [];
  total = 0;
  isLoading = false;
  errorMessage = '';
  /** Query the current results belong to (for the "No results for ..." message). */
  searchedFor = '';

  readonly minQueryLength = MIN_QUERY_LENGTH;

  ngOnInit(): void {
    this.search$
      .pipe(
        map(q => q.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        tap(() => (this.errorMessage = '')),
        switchMap(query => {
          if (query.length < MIN_QUERY_LENGTH) {
            this.isLoading = false;
            return of({ query, results: [], total: 0 } as Partial<MedicineSearchResponse>);
          }

          this.isLoading = true;

          // switchMap cancels the previous request automatically → stale results never overwrite new ones
          return this.medicineApi.searchMedicines(query, 1, 20).pipe(
            map(res => res as MedicineSearchResponse),
            catchError((err: HttpErrorResponse) => {
              this.errorMessage = err.status === 0
                ? 'Server nije dostupan. Provjerite da li je backend pokrenut.'
                : (err.error?.message || 'Pretraga trenutno nije dostupna. Pokušajte ponovo.');
              return of({ query, results: [], total: 0 } as Partial<MedicineSearchResponse>);
            }),
            finalize(() => (this.isLoading = false))
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(res => {
        this.medicines = res.results ?? [];
        this.total = res.total ?? 0;
        this.searchedFor = res.query ?? '';
      });
  }

  onSearchChange(query: string): void {
    this.search$.next(query ?? '');
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.search$.next('');
  }

  imageUrl(path: string | null | undefined): string {
    if (!path) return '';
    return `${environment.apiUrl}/${path.replace(/^\/+/, '')}`;
  }

  trackById(_: number, item: MedicineSearchItem): number {
    return item.id;
  }

  get showNoResults(): boolean {
    return !this.isLoading
      && !this.errorMessage
      && this.searchedFor.length >= MIN_QUERY_LENGTH
      && this.medicines.length === 0;
  }
}
