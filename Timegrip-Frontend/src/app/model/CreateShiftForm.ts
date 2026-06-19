import type { Employee } from './Employee';
import type { Role } from './Role';

export interface CreateShiftForm {
  employees: Employee[];
  roles: Role[];
}
