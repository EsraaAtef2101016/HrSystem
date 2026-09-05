export interface ParticipationStatus {
  isOptedIn: boolean;
  lastOptOutDate: string | null;
  cooldownEndDate: string | null;
}

export interface MessageResponse {
  status: string;
  message: string;
}