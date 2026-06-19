import type { ShiftBlock } from './ShiftBlock';

export interface ScheduleRow {
  employeeName: string;
  monday: ShiftBlock[];
  tuesday: ShiftBlock[];
  wednesday: ShiftBlock[];
  thursday: ShiftBlock[];
  friday: ShiftBlock[];
  saturday: ShiftBlock[];
  sunday: ShiftBlock[];
}
