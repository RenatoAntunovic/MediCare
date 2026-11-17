import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../../api-services/auth/register-api.service';
import { ToasterService } from '../../../core/services/toaster.service';

describe('RegisterComponent (wizard)', () => {
  let component: RegisterComponent;
  let authService: { register: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  const fillAll = () =>
    component.form.setValue({
      personal: { firstName: 'Amra', lastName: 'Hodžić', dateOfBirth: new Date(1995, 4, 17) },
      account: { userName: 'amra.h', email: 'amra@demo.com', password: 'Abcdefg1!', confirmPassword: 'Abcdefg1!' },
      contact: { phoneNumber: '061 123 456', address: 'Maršala Tita 12', city: 'Mostar' }
    });

  beforeEach(() => {
    authService = { register: vi.fn().mockReturnValue(of({})) };
    router = { navigate: vi.fn() };

    TestBed.configureTestingModule({
      declarations: [RegisterComponent],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
        { provide: ToasterService, useValue: { success: vi.fn() } }
      ]
    }).overrideComponent(RegisterComponent, { set: { template: '' } }); // class logic only

    component = TestBed.createComponent(RegisterComponent).componentInstance;
    component.ngOnInit();
  });

  it('each step has its own group, and a step is valid only when its fields are valid', () => {
    expect(component.form.controls['personal'].valid).toBe(false);
    component.form.get('personal')!.setValue({ firstName: 'Amra', lastName: 'Hodžić', dateOfBirth: new Date(1995, 4, 17) });
    expect(component.form.controls['personal'].valid).toBe(true);
    expect(component.form.controls['account'].valid).toBe(false); // other steps are still empty
  });

  it('step 2 is invalid while the passwords do not match', () => {
    fillAll();
    component.form.get('account.confirmPassword')!.setValue('different');
    expect(component.form.get('account.confirmPassword')!.hasError('mismatch')).toBe(true);
    expect(component.form.controls['account'].valid).toBe(false);
  });

  it('password strength follows the typed password', () => {
    expect(component.passwordStrength.score).toBe(0);
    component.form.get('account.password')!.setValue('abc');
    expect(component.passwordStrength.label).toBe('Slaba');
    component.form.get('account.password')!.setValue('Abcdefg1!');
    expect(component.passwordStrength.label).toBe('Jaka');
  });

  it('sends the same flat payload to the API as before the wizard', () => {
    fillAll();
    component.onSubmit();

    expect(authService.register).toHaveBeenCalledWith({
      firstName: 'Amra',
      lastName: 'Hodžić',
      userName: 'amra.h',
      email: 'amra@demo.com',
      password: 'Abcdefg1!',
      phoneNumber: '061 123 456',
      address: 'Maršala Tita 12',
      city: 'Mostar',
      dateOfBirth: '1995-05-17'
    });
    expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
  });

  it('does not call the API while any step is invalid', () => {
    component.onSubmit();
    expect(authService.register).not.toHaveBeenCalled();
  });
});
