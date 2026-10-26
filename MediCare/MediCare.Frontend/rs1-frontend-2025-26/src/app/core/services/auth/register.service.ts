export interface RegisterRequest {
  firstName: string;
  lastName: string;
  dateOfBirth: string; // or Date
  address: string;
  city: string;
  userName: string;
  email: string;
  password: string;
  phoneNumber: string;
}

// response
export interface RegisterResponse {
  id: number;          // user ID
  userName: string;
  email: string;
  role: string;        // "User"
}