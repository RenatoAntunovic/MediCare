import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RegisterCommand, RegisterCommandDto } from './register-api.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly apiUrl = `${environment.apiUrl}/api/auth`;

  constructor(private http: HttpClient) {}

  register(command: RegisterCommand): Observable<RegisterCommandDto> {
    return this.http.post<RegisterCommandDto>(
      `${this.apiUrl}/register`,
      command
    );
  }
}
