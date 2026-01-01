import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { AuthService } from '../../../api-services/auth/register-api.service';
import { RegisterCommand } from '../../../api-services/auth/register-api.model';
import { Router } from '@angular/router';
import { ToasterService } from '../../../core/services/toaster.service';

@Component({
  selector: 'app-register',
  standalone:false,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {

  form!: FormGroup;                     
  isLoading: boolean = false;           
  hidePassword: boolean = true;         
  hideConfirmPassword: boolean = true;  
  errorMessage: string = '';           

  constructor(
    private fb: FormBuilder, 
    private authService:AuthService,
    private router:Router,
    private toaster:ToasterService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
  firstName: ['', Validators.required],
  lastName: ['', Validators.required],
  userName: ['', Validators.required],
  email: ['', [Validators.required, Validators.email]],
  password: ['', Validators.required],
  confirmPassword: ['', Validators.required],
  phoneNumber: ['', Validators.required],
  dateOfBirth: [null, Validators.required],
  address: ['', Validators.required],
  city: ['', Validators.required],
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
  dateOfBirth: this.form.value.dateOfBirth
    .toISOString()
    .split('T')[0]
};

fetch('https://localhost:7260/api/auth/register', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(payload)
})
.then(res => res.json())
.then(data => console.log('Fetch test response:', data))
.catch(err => console.error('Fetch test error:', err));

 this.authService.register(payload).subscribe({
      next: (res) => {
        this.isLoading = false;
        // Prikaz poruke uspješne registracije
        this.toaster.success('Registracija uspješna! Možete se sada prijaviti.');
        
        // Automatski redirect na login stranicu
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err?.error?.message || 'Greška pri registraciji';
      }
    });
  }

}
