import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

/** Corresponds to: MyProfileDto.cs */
export interface MyProfileDto {
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  address: string;
  city: string;
  dateOfBirth: string; // "2002-05-15T00:00:00"
}

/** Corresponds to: UpdateMyProfileCommand.cs */
export interface UpdateMyProfileCommand {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  address: string;
  city: string;
  dateOfBirth: string; // "yyyy-MM-dd"
}

/** Corresponds to: ChangePasswordCommand.cs */
export interface ChangePasswordCommand {
  currentPassword: string;
  newPassword: string;
}

@Injectable({ providedIn: 'root' })
export class ProfileApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/Profile`;
  private http = inject(HttpClient);

  /** GET /api/Profile – the logged-in user's data */
  get(): Observable<MyProfileDto> {
    return this.http.get<MyProfileDto>(this.baseUrl);
  }

  /** PUT /api/Profile – save the logged-in user's data */
  update(command: UpdateMyProfileCommand): Observable<MyProfileDto> {
    return this.http.put<MyProfileDto>(this.baseUrl, command);
  }

    /** POST /api/Profile/change-password – signs the user out on all devices */
  changePassword(command: ChangePasswordCommand): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/change-password`, command);
  }
}