import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AssistantChatCommand, AssistantChatResponseDto } from './assistant-api.models';

@Injectable({
  providedIn: 'root'
})
export class AssistantApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/Assistant`;
  private http = inject(HttpClient);

  /**
   * POST /api/Assistant/chat
   * Patients and admins get different answers (decided by role on the server).
   */
  chat(command: AssistantChatCommand): Observable<AssistantChatResponseDto> {
    return this.http.post<AssistantChatResponseDto>(`${this.baseUrl}/chat`, command);
  }
}
