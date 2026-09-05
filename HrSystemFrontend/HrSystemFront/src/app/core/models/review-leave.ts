export interface ReviewLeaveResponse {
  id: string;
  status: string;
  startDate: string;
  endDate: string;
  employeeName: string;
  updatedAt: string;
}

export interface RejectionRequest {
  rejectionReason: string;
}

export interface MessageResponse {
  status: string;
  message: string;
}
