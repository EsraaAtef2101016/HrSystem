export interface LeavePolicyResponse {
id: string;
  leaveType: string | number;
  isEnabled: boolean;
  annualAllowance: number;
  maxConsecutiveDays: number;
  minNoticeDays: number;
  backdateDays: number;
  version: number;
}

export interface CreateLeavePolicyRequest {
  leaveType: string;
  annualAllowance: number;
  maxConsecutiveDays: number;
  minNoticeDays: number;
  backdateDays: number;
}

export interface UpdateLeavePolicyRequest {
  id: string;
  annualAllowance: number;
  maxConsecutiveDays: number;
  minNoticeDays: number;
  backdateDays: number;
}

export interface UpdatePolicyStatusRequest {
  id: string;
  isEnabled: boolean;
}