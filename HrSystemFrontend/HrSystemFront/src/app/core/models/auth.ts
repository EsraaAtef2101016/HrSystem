
export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserDto {
  userId: string;
  email: string;
  displayName: string;
  role: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: UserDto;
}

export enum UserRole {'Employee' , 'Manager' , 'Admin'}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  userRole: UserRole;
  managerId?: string | null;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  displayName: string;
  userRole: string;
  message: string;
}

