export interface PublicHolidayResponse {
  id: string;
  date: string; 
  name: string;
}

export interface PublicHolidayRequest {
  date: string;
  name: string;
}

export interface MessageResponse {
  status: string;
  message: string;
}