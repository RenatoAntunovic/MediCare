import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ForLaterDto,
  AddToForLaterCommand,
  DeleteForLaterCommand
} from './for-later-api.model';

@Injectable({
  providedIn: 'root',
})
export class ForLaterApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/ForLater`;
  private http = inject(HttpClient);

  /**
   * GET /Cart
   * Get the whole cart of the current user
   */
  getForLater(): Observable<ForLaterDto[]> {
    return this.http.get<ForLaterDto[]>(this.baseUrl);
  }

  /**
   * POST /Cart
   * Add an item to the cart
   */
  addToForLater(command: AddToForLaterCommand): Observable<{ forLaterId: number }> {
    return this.http.post<{ forLaterId: number }>(`${this.baseUrl}`, command);
  }


  /**
   * DELETE /Cart/{id}
   * Remove an item from the cart
   */
  deleteForLater(id: number): Observable<void> {
  return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

}
