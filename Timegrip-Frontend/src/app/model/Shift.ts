import type { ShiftAssignment } from './ShiftAssignment';
import type { ShiftRequirement } from './ShiftRequirement';

export interface Shift {
  shiftId: string;
  startTime: string;
  endTime: string;
  roleId: string;
  roleName?: string;
  isAssigned?: boolean;
  employeeRoleId?: string;
  shiftRequirements?: ShiftRequirement[];
  shiftAssignments?: ShiftAssignment[];
}
