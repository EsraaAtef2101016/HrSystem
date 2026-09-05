export interface LeaveRequest {
  id: string;
  employeeId: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  status: string;
  chargedDays: number;
  rejectionReason: string | null;
  policyVersionSnapshot: number;
  policyAllowanceSnapshot: number;
  createdAt: string;
}
export interface CreateUpdateLeaveRequest {
  leaveType: string;
  startDate: string;
  endDate: string;
}