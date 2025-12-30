import { Injectable, inject } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import {GetTreatmentsByIdQueryDto} from '../../../../../api-services/treatments/treatments-api.models';

/**
 * Service for creating and managing product forms.
 * Provides reusable form creation with validation for Add and Edit components.
 */
@Injectable()
export class TreatmentsFormService {
  private fb = inject(FormBuilder);

  /**
   * Create a product form with validation.
   * If product data is provided, form is pre-filled (edit mode).
   */
  createTreatmentsForm(treatments?: GetTreatmentsByIdQueryDto): FormGroup {
    return this.fb.group({
      name: [
        treatments?.serviceName ?? '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100)
        ]
      ],
      description: [
        treatments?.description ?? '',
        [Validators.maxLength(500)]
      ],
      price: [
        treatments?.price ?? 0,
        [
          Validators.required,
          Validators.min(0.01),
          Validators.max(1000000)
        ]
      ],
      categoryId: [Number(treatments?.treatmentsCategoryId ?? 0) || null, [Validators.required]]
    });
  }

  /**
   * Get validation error message for a form control.
   */
  getErrorMessage(form: FormGroup, controlName: string): string {
    const control = form.get(controlName);
    if (!control || !control.errors || !control.touched) {
      return '';
    }

    const errors = control.errors;

    if (errors['required']) {
      return 'This field is required';
    }
    if (errors['minlength']) {
      return `Minimum ${errors['minlength'].requiredLength} characters required`;
    }
    if (errors['maxlength']) {
      return `Maximum ${errors['maxlength'].requiredLength} characters allowed`;
    }
    if (errors['min']) {
      return `Minimum value is ${errors['min'].min}`;
    }
    if (errors['max']) {
      return `Maximum value is ${errors['max'].max}`;
    }
    if (errors['email']) {
      return 'Invalid email format';
    }

    return 'Invalid value';
  }
}
