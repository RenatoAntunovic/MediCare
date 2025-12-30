export interface RegisterRequest {
  firstName: string;
  lastName: string;
  dateOfBirth: string; // ili Date
  address: string;
  city: string;
  userName: string;
  email: string;
  password: string;
  phoneNumber: string;
}

// response
export interface RegisterResponse {
  id: number;          // ID korisnika
  userName: string;
  email: string;
  role: string;        // "User"
}