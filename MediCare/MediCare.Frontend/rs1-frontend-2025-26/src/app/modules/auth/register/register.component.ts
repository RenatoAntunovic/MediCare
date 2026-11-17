import { Component, OnInit } from '@angular/core';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { AuthService } from '../../../api-services/auth/register-api.service';
import { RegisterCommand } from '../../../api-services/auth/register-api.model';
import { Router } from '@angular/router';
import { ToasterService } from '../../../core/services/toaster.service';
import { formatDate } from '@angular/common';
import { notBlank, PHONE_PATTERN } from '../../../core/utils/form-validators';
import { startOfToday } from '../../../core/utils/date-utils';
import { getPasswordStrength, PasswordStrength } from '../../../core/utils/password-strength';

@Component({
  selector: 'app-register',
  standalone:false,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  providers: [{ provide: MAT_DATE_LOCALE, useValue: 'bs-BA' }] // dd.MM.yyyy, Monday first – like the other date fields
})
export class RegisterComponent implements OnInit {

  form!: FormGroup;                     
  isLoading: boolean = false;           
  hidePassword: boolean = true;         
  hideConfirmPassword: boolean = true;  
  errorMessage: string = '';           
  readonly maxBirthDate = startOfToday();
  readonly minBirthDate = new Date(1900, 0, 2);

  constructor(
    private fb: FormBuilder, 
    private authService:AuthService,
    private router:Router,
    private toaster:ToasterService
  ) {}

  ngOnInit(): void {
    // One FormGroup per wizard step, so a step can only be left when its own group is valid.
    // Same rules as RegisterCommandValidator on the backend.
    this.form = this.fb.group({
      personal: this.fb.group({
        firstName: ['', [Validators.required, notBlank, Validators.maxLength(50)]],
        lastName: ['', [Validators.required, notBlank, Validators.maxLength(50)]],
        dateOfBirth: [null, Validators.required],
      }),
      account: this.fb.group({
        userName: ['', [Validators.required, notBlank, Validators.minLength(3), Validators.maxLength(50)]],
        email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
        password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
        confirmPassword: ['', Validators.required],
      }, {
        validators: this.passwordsMatchValidator
      }),
      contact: this.fb.group({
        phoneNumber: ['', [Validators.required, Validators.pattern(PHONE_PATTERN)]],
        address: ['', [Validators.required, notBlank, Validators.maxLength(100)]],
        city: ['', [Validators.required, notBlank, Validators.maxLength(50)]],
      }),
    });
  }

  /** Strength of the typed password, shown under the password field */
  get passwordStrength(): PasswordStrength {
    return getPasswordStrength(this.form.get('account.password')?.value);
  }

 passwordsMatchValidator(control: AbstractControl) {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword');

  if (!confirmPassword) return null;

  if (password !== confirmPassword.value) {
    confirmPassword.setErrors({ mismatch: true });
  } else {
    const errors = confirmPassword.errors;
    if (errors) {
      delete errors['mismatch'];
      if (Object.keys(errors).length === 0) {
        confirmPassword.setErrors(null);
      }
    }
  }

  return null;
}


  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;  

    // The API still receives one flat object, the step groups are only a UI structure
    const { personal, account, contact } = this.form.getRawValue();
    const payload: RegisterCommand = {
      firstName: personal.firstName,
      lastName: personal.lastName,
      userName: account.userName,
      email: account.email,
      password: account.password,
      phoneNumber: contact.phoneNumber,
      address: contact.address,
      city: contact.city,
      dateOfBirth: formatDate(personal.dateOfBirth, 'yyyy-MM-dd', 'en')
    };



 this.authService.register(payload).subscribe({
      next: (res) => {
        this.isLoading = false;
        // Show the registration success message
        this.toaster.success('Registracija uspješna! Možete se sada prijaviti.');
        
        // Automatically redirect to the login page
       this.router.navigate(['/auth/login']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err?.error?.message || 'Greška pri registraciji';
      }
    });
  }

}
