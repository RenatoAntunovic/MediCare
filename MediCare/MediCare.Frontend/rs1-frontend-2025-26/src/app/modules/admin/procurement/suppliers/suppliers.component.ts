import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { Sort } from '@angular/material/sort';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { largePaging } from '../../../../core/models/paging/paging-utils';
import { toLocalDateString } from '../../../../core/utils/date-utils';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';
import { DialogButton } from '../../../shared/models/dialog-config.model';
import { SuppliersApiService } from '../../../../api-services/suppliers/suppliers-api.service';
import { ListSuppliersQueryDto, ListSuppliersRequest } from '../../../../api-services/suppliers/suppliers-api.models';
import { MedicineApiService } from '../../../../api-services/medicine/medicine-api.service';
import { ListMedicineQueryDto } from '../../../../api-services/medicine/medicine-api.models';
import { SupplierUpsertComponent, SupplierDialogData } from './supplier-upsert/supplier-upsert.component';

@Component({
  selector: 'app-suppliers',
  standalone: false,
  templateUrl: './suppliers.component.html',
  styleUrl: './suppliers.component.scss'
})
export class SuppliersComponent
  extends BaseListPagedComponent<ListSuppliersQueryDto, ListSuppliersRequest>
  implements OnInit {

  private api = inject(SuppliersApiService);
  private medicineApi = inject(MedicineApiService);
  private dialog = inject(MatDialog);
  private toaster = inject(ToasterService);
  private dialogHelper = inject(DialogHelperService);
  private destroyRef = inject(DestroyRef);

  displayedColumns: string[] = ['companyName', 'contactName', 'city', 'medicinesCount', 'createdAtUtc', 'isEnabled', 'actions'];

  // ==================== FILTERS (5, all applied on the server) ====================
  searchText = '';
  cityFilter: string | null = null;
  statusFilter: 'all' | 'active' | 'inactive' = 'all';
  medicineFilter: number | null = null;
  createdFrom: Date | null = null;

  /** Data for the filter dropdowns */
  cities: string[] = [];
  medicines: ListMedicineQueryDto[] = [];

  /** The "added from" date cannot be in the future (same rule as ListSuppliersQueryValidator). */
  readonly today = new Date();

  isSaving = false;

  private search$ = new Subject<string>();

  constructor() {
    super();
    this.request = new ListSuppliersRequest();
    this.request.paging.pageSize = 10;
  }

  ngOnInit(): void {
    this.loadLookups();
    this.initList();

    // Debounced text search – no request on every key press
    this.search$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.applyFilters());
  }

  // ==================== DATA ====================

  private loadLookups(): void {
    this.api.cities().subscribe({
      next: (cities) => (this.cities = cities),
      error: (err) => console.error('Failed to load cities:', err)
    });

    this.medicineApi.list({ paging: largePaging } as any).subscribe({
      next: (response) => (this.medicines = response.items),
      error: (err) => console.error('Failed to load medicines:', err)
    });
  }

  protected loadPagedData(): void {
    this.startLoading();

    this.api.list(this.request).subscribe({
      next: (response) => {
        this.handlePageResult(response);
        this.stopLoading();
      },
      error: (err) => {
        console.error('Load suppliers error:', err);
        this.stopLoading(this.extractError(err, 'Učitavanje dobavljača nije uspjelo.'));
      }
    });
  }

  // ==================== FILTERS ====================

  onSearchInput(value: string): void {
    this.search$.next((value ?? '').trim());
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  get isDateInvalid(): boolean {
    return !!this.createdFrom && this.createdFrom > this.today;
  }

  private applyFilters(): void {
    // Frontend validation: a future date is not sent to the server
    if (this.isDateInvalid) return;

    this.request.search = this.searchText.trim() || null;
    this.request.city = this.cityFilter;
    this.request.onlyEnabled = this.statusFilter === 'all' ? null : this.statusFilter === 'active';
    this.request.medicineId = this.medicineFilter;
    this.request.createdFrom = this.createdFrom ? toLocalDateString(this.createdFrom) : null;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  get hasActiveFilters(): boolean {
    return !!this.searchText.trim() || this.cityFilter !== null || this.statusFilter !== 'all' ||
      this.medicineFilter !== null || this.createdFrom !== null;
  }

  clearFilters(): void {
    this.searchText = '';
    this.cityFilter = null;
    this.statusFilter = 'all';
    this.medicineFilter = null;
    this.createdFrom = null;
    this.applyFilters();
  }

  onSortChange(sort: Sort): void {
    this.request.sortBy = sort.direction ? sort.active : null;
    this.request.sortDirection = sort.direction || null;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  // ==================== CRUD ====================

  onCreate(): void {
    this.openDialog({ mode: 'create' });
  }

  onEdit(supplier: ListSuppliersQueryDto): void {
    this.openDialog({ mode: 'edit', supplierId: supplier.id });
  }

  private openDialog(data: SupplierDialogData): void {
    const dialogRef = this.dialog.open(SupplierUpsertComponent, {
      width: '640px',
      maxWidth: '95vw',
      autoFocus: true,
      data: { ...data, medicines: this.medicines }
    });

    dialogRef.afterClosed().subscribe((saved: boolean) => {
      if (saved) {
        this.toaster.success(data.mode === 'create' ? 'Dobavljač je dodan.' : 'Dobavljač je ažuriran.');
        this.loadLookups(); // a new city may have been added
        this.loadPagedData();
      }
    });
  }

  onDelete(supplier: ListSuppliersQueryDto): void {
    this.dialogHelper.confirmDelete(supplier.companyName).subscribe(result => {
      if (result?.button !== DialogButton.DELETE) return;

      this.isSaving = true;
      this.api.delete(supplier.id).subscribe({
        next: () => {
          this.isSaving = false;
          this.toaster.success('Dobavljač je obrisan.');
          // Last row on the page deleted → go one page back
          if (this.items.length === 1 && this.paging.page > 1) this.paging.page--;
          this.loadPagedData();
        },
        error: (err) => {
          this.isSaving = false;
          this.toaster.error(this.extractError(err, 'Brisanje dobavljača nije uspjelo.'));
        }
      });
    });
  }

  onToggleStatus(supplier: ListSuppliersQueryDto): void {
    if (this.isSaving) return;
    this.isSaving = true;

    const request$ = supplier.isEnabled ? this.api.disable(supplier.id) : this.api.enable(supplier.id);

    request$.subscribe({
      next: () => {
        supplier.isEnabled = !supplier.isEnabled;
        this.isSaving = false;
      },
      error: (err) => {
        this.isSaving = false;
        this.toaster.error(this.extractError(err, 'Promjena statusa nije uspjela.'));
      }
    });
  }

  private extractError(err: unknown, fallback: string): string {
    if (err instanceof HttpErrorResponse) {
      if (err.status === 0) return 'Server nije dostupan.';
      if (err.status === 401 || err.status === 403) return 'Nemate prava za ovu akciju.';
      return err.error?.message || fallback;
    }
    return fallback;
  }
}
