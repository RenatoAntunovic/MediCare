import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  Reservation, 
  CreateReservationRequest, 
  UpdateReservationRequest, 
  ChangeReservationStatusRequest 
} from '@shared/models/reservation.model';

@Injectable({
  providedIn: 'root'
})
export class ReservationsService {
  private baseUrl = 'api/Reservations';

  constructor(private http: HttpClient) {}

  getAllReservations(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(this.baseUrl);
  }

  getReservationById(id: number): Observable<Reservation> {
    return this.http.get<Reservation>(`${this.baseUrl}/${id}`);
  }

  createReservation(request: CreateReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(this.baseUrl, request);
  }

  updateReservation(id: number, request: UpdateReservationRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  changeStatus(id: number, request: ChangeReservationStatusRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/status`, request);
  }
}
