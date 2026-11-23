import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { SuppliersApiService } from '../../../../../api-services/suppliers/suppliers-api.service';
import { GetSupplierByIdQueryDto, UpsertSupplierCommand } from '../../../../../api-services/suppliers/suppliers-api.models';
import { ListMedicineQueryDto } from '../../../../../api-services/medicine/medicine-api.models';
import { Observable } from 'rxjs';

export interface SupplierDialogData {
  mode: 'create' | 'edit';
  supplierId?: number;
  /** Medicines for the "supplied medicines" multi-select (already loaded by the list). */
  medicines?: ListMedicineQueryDto[];
}

/** Same pattern as SupplierFieldsValidator.PhonePattern on the backend. */
const PHONE_PATTERN = /^[0-9+\-\s/()]{6,30}$/;

@Component({
  selector: 'app-supplier-upsert',
  standalone: false,
  templateUrl: './supplier-upsert.component.html',
  styleUrl: './supplier-upsert.component.scss'
})
export class SupplierUpsertComponent implements OnInit {
  private dialogRef = inject(MatDialogRef<SupplierUpsertComponent>);
  readonly data = inject<SupplierDialogData>(MAT_DIALOG_DATA);
  private api = inject(SuppliersApiService);
  private fb = inject(FormBuilder);

  form!: FormGroup;
  isLoading = false;
  isEditMode = false;
  /** Error returned by the backend (e.g. duplicate company name), shown above the buttons. */
  serverError: string | null = null;

  get medicines(): ListMedicineQueryDto[] {
    return this.data.medicines ?? [];
  }

  ngOnInit(): void {
    this.isEditMode = this.data.mode === 'edit';

    if (this.isEditMode && this.data.supplierId) {
      this.isLoading = true;
      this.api.getById(this.data.supplierId).subscribe({
        next: (supplier) => {
          this.form = this.createForm(supplier);
          this.isLoading = false;
        },
        error: (err) => {
          this.isLoading = false;
          this.serverError = this.extractError(err, 'Učitavanje dobavljača nije uspjelo.');
        }
      });
    } else {
      this.form = this.createForm();
    }
  }

  /** Frontend validation – the same rules as SupplierFieldsValidator on the backend. */
  private createForm(s?: GetSupplierByIdQueryDto): FormGroup {
    return this.fb.nonNullable.group({
      companyName: [s?.companyName ?? '', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      contactName: [s?.contactName ?? '', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      email: [s?.email ?? '', [Validators.required, Validators.email, Validators.maxLength(100)]],
      phone: [s?.phone ?? '', [Validators.required, Validators.pattern(PHONE_PATTERN)]],
      address: [s?.address ?? '', [Validators.required, Validators.maxLength(200)]],
      city: [s?.city ?? '', [Validators.required, Validators.maxLength(50)]],
      isEnabled: [s?.isEnabled ?? true],
      medicineIds: [s?.medicineIds ?? [] as number[]]
    });
  }

  hasError(control: string): boolean {
    const c = this.form?.get(control);
    return !!c && c.invalid && (c.touched || c.dirty);
  }

  /** Translation key + params for the first error of a control. */
  errorKey(control: string): { key: string; params?: any } {
    const errors = this.form.get(control)?.errors ?? {};
    if (errors['required']) return { key: 'SUPPLIERS.FORM.ERRORS.REQUIRED' };
    if (errors['minlength']) return { key: 'SUPPLIERS.FORM.ERRORS.MIN_LENGTH', params: { n: errors['minlength'].requiredLength } };
    if (errors['maxlength']) return { key: 'SUPPLIERS.FORM.ERRORS.MAX_LENGTH', params: { n: errors['maxlength'].requiredLength } };
    if (errors['email']) return { key: 'SUPPLIERS.FORM.ERRORS.EMAIL' };
    if (errors['pattern']) return { key: 'SUPPLIERS.FORM.ERRORS.PHONE' };
    return { key: 'SUPPLIERS.FORM.ERRORS.INVALID' };
  }

  onSubmit(): void {
    if (!this.form || this.isLoading) return;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const command: UpsertSupplierCommand = {
      companyName: v.companyName.trim(),
      contactName: v.contactName.trim(),
      email: v.email.trim(),
      phone: v.phone.trim(),
      address: v.address.trim(),
      city: v.city.trim(),
      isEnabled: v.isEnabled,
      medicineIds: v.medicineIds
    };

    this.isLoading = true;
    this.serverError = null;

        // create returns { id }, update returns nothing – the result is not needed here
    const request$: Observable<unknown> = this.isEditMode && this.data.supplierId
      ? this.api.update(this.data.supplierId, command)
      : this.api.create(command);

    request$.subscribe({
      next: () => {
        this.isLoading = false;
        this.dialogRef.close(true);
      },
           error: (err: unknown) => {
        this.isLoading = false;
        // Backend validation / conflict (e.g. the company name or email already exists)
        this.serverError = this.extractError(err, 'Spremanje dobavljača nije uspjelo.');
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  private extractError(err: unknown, fallback: string): string {
    if (err instanceof HttpErrorResponse) {
      if (err.status === 0) return 'Server nije dostupan.';
      return err.error?.message || fallback;
    }
    return fallback;
  }
}
