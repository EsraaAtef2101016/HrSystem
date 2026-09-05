export interface UserProfileResponse {
  id: string;
  name: string;
  email: string;
  role: string;
  isActive: boolean;
  managerId: string | null;
}