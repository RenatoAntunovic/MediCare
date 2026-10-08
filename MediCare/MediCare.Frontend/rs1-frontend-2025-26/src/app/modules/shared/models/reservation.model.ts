export interface Reservation {
  id: number;
  userId: number;
  treatmentId: number;
  treatmentName: string;
  treatmentDescription?: string;
  reservationDate: string;   // "2026-10-20T00:00:00"
  reservationTime: string;   // "14:00:00"
  orderStatus: string;
  price: number;
  notes?: string | null;
}

export interface CreateReservationRequest {
  treatmentId: number;
  reservationDate: string;   // "yyyy-MM-dd" (local date, WITHOUT time and time zone)
  reservationTime: string;   // "HH:mm:ss"
  notes?: string | null;
}

export interface CreateReservationResponse {
  reservationId: number;
  message: string;
}

export interface UpdateReservationRequest {
  treatmentId: number;
  reservationDate: string;   // "yyyy-MM-dd"
  reservationTime: string;   // "HH:mm:ss"
}

export interface ChangeReservationStatusRequest {
  newStatusId: number;
}
