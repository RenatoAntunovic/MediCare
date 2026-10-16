import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Shared form rules – they must match the FluentValidation rules on the backend
 * (RegisterCommandValidator, UpdateMyProfileCommandValidator).
 */

/** Optional "+", then digits, spaces, "-" or "/" – e.g. 061 123 456, +387 61-123-456 */
export const PHONE_PATTERN = /^\+?[\d\s\-/]{9,20}$/;

/**
 * Like Validators.required, but "   " (only spaces) also counts as empty.
 * Returns the same { required: true } error, so the existing "… is required" messages show.
 */
export const notBlank: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  return typeof value === 'string' && value.length > 0 && value.trim().length === 0
    ? { required: true }
    : null;
};

/**
 * The control must differ from a sibling control (e.g. new password ≠ current password).
 * Re-validate this control when the sibling changes.
 */
export function differentFrom(siblingName: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const sibling = control.parent?.get(siblingName)?.value;
    return control.value && sibling && control.value === sibling ? { sameAsCurrent: true } : null;
  };
}
