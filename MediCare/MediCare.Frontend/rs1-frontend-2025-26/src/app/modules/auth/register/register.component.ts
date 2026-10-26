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
    this.form = this.fb.group({
  // Same rules as RegisterCommandValidator on the backend
  firstName: ['', [Validators.required, notBlank, Validators.maxLength(50)]],
  lastName: ['', [Validators.required, notBlank, Validators.maxLength(50)]],
  userName: ['', [Validators.required, notBlank, Validators.minLength(3), Validators.maxLength(50)]],
  email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
  password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
  confirmPassword: ['', Validators.required],
  phoneNumber: ['', [Validators.required, Validators.pattern(PHONE_PATTERN)]],
  dateOfBirth: [null, Validators.required],
  address: ['', [Validators.required, notBlank, Validators.maxLength(100)]],
  city: ['', [Validators.required, notBlank, Validators.maxLength(50)]],
}, {
  validators: this.passwordsMatchValidator
});

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

const payload: RegisterCommand = {
  firstName: this.form.value.firstName,
  lastName: this.form.value.lastName,
  userName: this.form.value.userName,
  email: this.form.value.email,
  password: this.form.value.password,
  phoneNumber: this.form.value.phoneNumber,
  address: this.form.value.address,
  city: this.form.value.city,
  dateOfBirth: formatDate(this.form.value.dateOfBirth, 'yyyy-MM-dd', 'en')
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
