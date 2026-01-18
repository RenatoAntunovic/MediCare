export interface Reservation {
  id: number;
  userId: number;
  treatmentId: number;
  treatmentName: string;
  treatmentDescription: string;
  reservationDate: string;   // "2026-01-07T10:00:00"
  reservationTime: string;   // "14:00:00"
  orderStatus: string;
  price: number;
}

export interface CreateReservationRequest {
  treatmentId: number;
  reservationDate: string;   // ISO string
  reservationTime: string;   // "HH:mm:ss"
}

export interface UpdateReservationRequest {
  treatmentId: number;
  reservationDate: string;
  reservationTime: string;
}

export interface ChangeReservationStatusRequest {
  newStatusId: number;
}
