import { Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BaseComponent } from '../../../core/components/base-classes/base-component';
import { AuthFacadeService } from '../../../core/services/auth/auth-facade.service';
import { LoginCommand } from '../../../api-services/auth/auth-api.model';
import { CurrentUserService } from '../../../core/services/auth/current-user.service';
import { AuthApiService } from '../../../api-services/auth/auth-api.service';
import { CurrentUserDto } from '../../../core/services/auth/current-user.dto';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent extends BaseComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthFacadeService);
  private router = inject(Router);
  private currentUser = inject(CurrentUserService);
  private authApi = inject(AuthApiService);
  hidePassword = true;

  form = this.fb.group({
    email: ['admin@market.com', [Validators.required, Validators.email]],
    password: ['Admin', [Validators.required]],
    rememberMe: [false],
  });

  loginAttempts = 0;
  lastAttemptTime = 0;
  maxAttempts = 5;
  blockTimeMs = 30000; // 30 sekundi

  onSubmit(): void {
     const now = Date.now();

  // Resetuj pokušaje ako je prošlo više od blockTimeMs
  if (now - this.lastAttemptTime > this.blockTimeMs) {
    this.loginAttempts = 0;
  }

  this.lastAttemptTime = now;
  this.loginAttempts++;

  if (this.loginAttempts > this.maxAttempts) {
    this.stopLoading('Too many login attempts. Please wait a while.');
    return;
  }

    if (this.form.invalid || this.isLoading) return;

    this.startLoading();

    const payload: LoginCommand = {
      email: this.form.value.email ?? '',
      password: this.form.value.password ?? '',
      fingerprint: null,
    };

    this.auth.login(payload).subscribe({
      next: (loggedInUser: CurrentUserDto) => {
        this.stopLoading();

         this.authApi.setCurrentUserId(loggedInUser.userId); // ← OVDJE

    console.log('Logged in user ID:', loggedInUser.userId);

        const target = this.currentUser.getDefaultRoute();
        console.log('DEFAULT ROUTE:', target);
        this.router.navigate([target]);
      },
      error: (err) => {
        this.stopLoading('Invalid credentials. Please try again.');
        console.error('Login error:', err);
      },
    });
  }
}
