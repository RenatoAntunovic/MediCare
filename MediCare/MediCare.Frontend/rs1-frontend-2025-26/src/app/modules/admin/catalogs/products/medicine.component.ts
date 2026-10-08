import { AfterViewInit, Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import {
  ListMedicineRequest,
  ListMedicineQueryDto
} from '../../../../api-services/medicine/medicine-api.models';
import { MedicineApiService } from '../../../../api-services/medicine/medicine-api.service';
import { MedicineCategoriesApiService } from '../../../../api-services/medicine-categories/medicine-categories-api.service';
import { ListMedicineCategoriesQueryDto } from '../../../../api-services/medicine-categories/medicine-categories-api.model';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { largePaging } from '../../../../core/models/paging/paging-utils';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';
import { DialogButton } from '../../../shared/models/dialog-config.model';
import { environment } from '../../../../../environments/environment';

type InlineEditForm = FormGroup<{
  name: FormControl<string>;
  price: FormControl<number>;
  weight: FormControl<number>;
  medicineCategoryId: FormControl<number>;
}>;

@Component({
  selector: 'app-medicine',
  standalone: false,
  templateUrl: './medicine.component.html',
  styleUrl: './medicine.component.scss'
})
export class MedicineComponent
  extends BaseListPagedComponent<ListMedicineQueryDto, ListMedicineRequest>
  implements OnInit, AfterViewInit {

  private api = inject(MedicineApiService);
  private categoriesApi = inject(MedicineCategoriesApiService);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private dialogHelper = inject(DialogHelperService);
  private fb = inject(FormBuilder);
  private destroyRef = inject(DestroyRef);

  displayedColumns: string[] = [
    'imageFile',
    'name',
    'medicineCategoryName',
    'price',
    'weight',
    'isEnabled',
    'actions'
  ];

  dataSource = new MatTableDataSource<ListMedicineQueryDto>([]);
  @ViewChild(MatSort) sort!: MatSort;

  categories: ListMedicineCategoriesQueryDto[] = [];

  // ==================== INLINE EDITING ====================
  editingRowId: number | null = null;
  editForm: InlineEditForm | null = null;
  /** Saving a row or changing status – does NOT hide the table (unlike isLoading). */
  isSaving = false;
  /** Backend error for the row being edited (e.g. a duplicate-name conflict). */
  rowError: string | null = null;

  /** Debounced search instead of a request on every key press. */
  private search$ = new Subject<string>();

  constructor() {
    super();
    this.request = new ListMedicineRequest();
  }

  ngOnInit(): void {
    this.loadCategories();
    this.initList();

    this.search$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.request.paging.page = 1;
        this.loadPagedData();
      });
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  // ==================== DATA ====================

  private loadCategories(): void {
    this.categoriesApi.list({ onlyEnabled: true, paging: largePaging }).subscribe({
      next: (response) => (this.categories = response.items),
      error: (err) => console.error('Failed to load categories:', err)
    });
  }

  protected loadPagedData(): void {
    this.cancelEditing();
    this.startLoading();

    this.api.list(this.request).subscribe({
      next: (response) => {
        this.items = response.items;
        this.dataSource.data = this.items;

        // The backend returns "total" while the frontend model expects "totalItems" – support both.
        const total = (response as any).total ?? response.totalItems ?? 0;
        this.totalItems = total;
        this.totalPages = Math.max(1, Math.ceil(total / this.request.paging.pageSize));

        this.stopLoading();
      },
      error: (err) => {
        console.error('Load error:', err);
        this.stopLoading('Učitavanje lijekova nije uspjelo.');
      }
    });
  }

  onSearchInput(value: string): void {
    this.search$.next((value ?? '').trim());
  }

  onSearch(): void {
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  // ==================== NAVIGATION / ACTIONS ====================

  onCreate(): void {
    this.router.navigate(['/admin/products/add']);
  }

  onEdit(medicine: ListMedicineQueryDto): void {
    this.router.navigate(['/admin/products', medicine.id, 'edit']);
  }

  onDelete(medicine: ListMedicineQueryDto): void {
    this.dialogHelper.medicine.confirmDelete(medicine.name).subscribe(result => {
      if (result && result.button === DialogButton.DELETE) {
        this.performDelete(medicine);
      }
    });
  }

  private performDelete(medicine: ListMedicineQueryDto): void {
    this.isSaving = true;

    this.api.delete(medicine.id).subscribe({
      next: () => {
        this.isSaving = false;
        this.dialogHelper.medicine.showDeleteSuccess().subscribe();
        this.loadPagedData();
      },
      error: (err) => {
        this.isSaving = false;
        console.error('Delete medicine error:', err);
        this.dialogHelper.showError('DIALOGS.TITLES.ERROR', 'MEDICINE.DIALOGS.ERROR_DELETE').subscribe();
      }
    });
  }

  onToggleStatus(medicine: ListMedicineQueryDto): void {
    if (this.isSaving) return;
    this.isSaving = true;

    const request$ = medicine.isEnabled
      ? this.api.disable(medicine.id)
      : this.api.enable(medicine.id);

    request$.subscribe({
      next: () => {
        medicine.isEnabled = !medicine.isEnabled;
        this.isSaving = false;
      },
      error: (err) => {
        console.error('Toggle status error:', err);
        this.isSaving = false;
        this.toaster.error(this.extractError(err, 'Promjena statusa nije uspjela.'));
      }
    });
  }

  // ==================== INLINE EDITING ====================

  startEditing(medicine: ListMedicineQueryDto): void {
    if (this.isSaving || this.editingRowId === medicine.id) return;

    // If another row with unsaved changes is being edited – ask before discarding
    if (this.editForm?.dirty && !confirm('Imate nesnimljene izmjene. Odbaciti ih?')) {
      return;
    }

    this.rowError = null;
    this.editingRowId = medicine.id;

    this.editForm = this.fb.nonNullable.group({
      name: [medicine.name, [Validators.required, Validators.minLength(3), Validators.maxLength(150)]],
      price: [medicine.price, [Validators.required, Validators.min(0.01)]],
      weight: [medicine.weight, [Validators.required, Validators.min(1)]],
      medicineCategoryId: [medicine.medicineCategoryId, [Validators.required]]
    });

    // Move the keyboard focus into the first field once it is rendered, so Enter (save) and
    // Esc (cancel) work right after the double click without having to click into the field first.
    setTimeout(() => {
      (document.querySelector('.mc-editing-row .inline-edit-field input') as HTMLElement | null)?.focus();
    });
  }

  saveRow(medicine: ListMedicineQueryDto): void {
    if (!this.editForm || this.isSaving) return;

    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      this.toaster.error('Ispravite označena polja.');
      return;
    }

    if (!this.editForm.dirty) {
      this.cancelEditing(); // nothing changed → no server call
      return;
    }

    const value = this.editForm.getRawValue();
    const name = value.name.trim();

    // Send ALL fields that UpdateMedicineCommand expects.
    // Description and status are NOT edited inline, so send the existing values.
    // The image is not sent → the backend keeps the existing one.
    const formData = new FormData();
    formData.append('name', name);
    formData.append('description', medicine.description ?? '');
    formData.append('price', String(value.price));
    formData.append('medicineCategoryId', String(value.medicineCategoryId));
    formData.append('weight', String(value.weight));
    formData.append('isEnabled', String(medicine.isEnabled));

    this.isSaving = true;
    this.rowError = null;

    this.api.updateFormData(medicine.id, formData).subscribe({
      next: () => {
        medicine.name = name;
        medicine.price = value.price;
        medicine.weight = value.weight;
        medicine.medicineCategoryId = value.medicineCategoryId;

        const category = this.categories.find(c => c.id === value.medicineCategoryId);
        if (category) {
          medicine.medicineCategoryName = category.name;
        }

        // Refresh the table so sorting works with the new values
        this.dataSource.data = [...this.items];

        this.isSaving = false;
        this.editingRowId = null;
        this.editForm = null;
        this.toaster.success('Lijek je ažuriran.');
      },
      error: (err) => {
        console.error('Update error:', err);
        this.isSaving = false;
        this.rowError = this.extractError(err, 'Ažuriranje lijeka nije uspjelo.');
        this.toaster.error(this.rowError);
      }
    });
  }

  cancelEditing(): void {
    this.editingRowId = null;
    this.editForm = null;
    this.rowError = null;
  }

  isEditing(medicineId: number): boolean {
    return this.editingRowId === medicineId;
  }

  /** Enter = save, Escape = cancel (on any field in the row). */
  onEditKeydown(event: KeyboardEvent, medicine: ListMedicineQueryDto): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.saveRow(medicine);
    } else if (event.key === 'Escape') {
      event.preventDefault();
      this.cancelEditing();
    }
  }

  hasError(controlName: keyof InlineEditForm['controls']): boolean {
    const control = this.editForm?.controls[controlName];
    return !!(control && control.invalid && (control.touched || control.dirty));
  }

  getErrorMessage(controlName: keyof InlineEditForm['controls']): string {
    const errors = this.editForm?.controls[controlName]?.errors;
    if (!errors) return '';

    if (errors['required']) return 'Obavezno polje';
    if (errors['minlength']) return `Najmanje ${errors['minlength'].requiredLength} znaka`;
    if (errors['maxlength']) return `Najviše ${errors['maxlength'].requiredLength} znakova`;
    if (errors['min']) return `Najmanja vrijednost je ${errors['min'].min}`;
    return 'Neispravna vrijednost';
  }

  getFormControl(controlName: keyof InlineEditForm['controls']): FormControl<any> {
    return this.editForm!.controls[controlName] as FormControl<any>;
  }

  imageUrl(path: string | null | undefined): string {
    if (!path) return '';
    return `${environment.apiUrl}/${path.replace(/^\/+/, '')}`;
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
