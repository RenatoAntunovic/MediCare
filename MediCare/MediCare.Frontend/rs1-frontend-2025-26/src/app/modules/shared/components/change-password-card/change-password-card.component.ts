import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { ProfileApiService } from '../../../../api-services/profile/profile-api.service';
import { AuthFacadeService } from '../../../../core/services/auth/auth-facade.service';
import { ToasterService } from '../../../../core/services/toaster.service';

/** "New password" and "Confirm" must match (error is set on the form group) */
function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const newPassword = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return newPassword && confirm && newPassword !== confirm ? { mismatch: true } : null;
}

/**
 * "Security" card on the Settings page: change password.
 * After a successful change the user is signed out everywhere and has to log in with the new password.
 */
@Component({
  selector: 'app-change-password-card',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './change-password-card.component.html',
  styleUrl: './change-password-card.component.scss'
})
export class ChangePasswordCardComponent {
  private fb = inject(FormBuilder);
  private api = inject(ProfileApiService);
  private auth = inject(AuthFacadeService);
  private toaster = inject(ToasterService);
  private router = inject(Router);

  isSaving = false;
  serverError = '';
  hideCurrent = true;
  hideNew = true;

  // Same rules as ChangePasswordCommandValidator on the backend (+ confirmation)
  form = this.fb.nonNullable.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
      confirmPassword: ['', Validators.required]
    },
    { validators: passwordsMatch }
  );

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { currentPassword, newPassword } = this.form.getRawValue();
    this.isSaving = true;
    this.serverError = '';

    this.api.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.toaster.success('Lozinka je promijenjena. Prijavite se novom lozinkom.');
        // All sessions were revoked on the server – sign out here too
        this.auth.logout().subscribe(() => this.router.navigate(['/auth/login']));
      },
      error: err => {
        this.isSaving = false;
        if (err.status === 429) return; // message already shown by the rate-limit interceptor
        this.serverError = err.error?.message || 'Lozinka nije promijenjena.';
      }
    });
  }
}