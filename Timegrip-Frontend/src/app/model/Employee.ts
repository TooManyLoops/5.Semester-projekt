import type { EmployeeRole } from './EmployeeRole';

export interface Employee {
  employeeId: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  employeeStatus: number;
  hiredAt?: string;
  roles?: EmployeeRole[];
  rolesText?: string;
}
