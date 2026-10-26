import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Reservation,
  CreateReservationRequest,
  CreateReservationResponse,
  UpdateReservationRequest,
  ChangeReservationStatusRequest
} from '@shared/models/reservation.model';

@Injectable({
  providedIn: 'root'
})
export class ReservationsService {
  private readonly baseUrl = `${environment.apiUrl}/api/Reservations`;
  private http = inject(HttpClient);

  /** GET /api/Reservations – reservations of the signed-in user */
  getMyReservations(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(this.baseUrl);
  }

  getReservationById(id: number): Observable<Reservation> {
    return this.http.get<Reservation>(`${this.baseUrl}/${id}`);
  }

  /**
   * GET /api/Reservations/availability?treatmentId=1&date=2026-10-20
   * Returns the taken slots for that day, e.g. ["09:00", "13:30"].
   */
  getTakenSlots(treatmentId: number, date: string): Observable<string[]> {
    const params = new HttpParams()
      .set('treatmentId', treatmentId)
      .set('date', date);

    return this.http.get<string[]>(`${this.baseUrl}/availability`, { params });
  }

  createReservation(request: CreateReservationRequest): Observable<CreateReservationResponse> {
    return this.http.post<CreateReservationResponse>(this.baseUrl, request);
  }

  updateReservation(id: number, request: UpdateReservationRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  /** PUT /api/Reservations/{id}/change-status (Admin only) */
  changeStatus(id: number, request: ChangeReservationStatusRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/change-status`, request);
  }
}
