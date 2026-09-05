// 1. Updated Models (employee-dashboard.model.ts)
export enum LeaveType {
  Vacation = 'Vacation',
  DayOff = 'DayOff',
  SickLeave = 'SickLeave'
}

export interface EmployeeParticipationStatus {
  isOptedIn: boolean;
  lastOptOutDate: string | null;
  cooldownEndDate: string | null;
}

export interface LeaveBalance {
  leaveType: LeaveType;
  initialAllowance: number;
  usedDays: number;
  reservedDays: number;
  availableDays: number;
  year: number;
}