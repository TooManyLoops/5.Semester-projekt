import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-shifts',
  standalone: false,
  templateUrl: './shifts.html',
  styleUrl: './shifts.css'
})
export class Shifts implements OnInit {

  shifts: any[] = [];
  employees: any[] = [];
  employeeRoles: any[] = [];
  roles: any[] = [];
  employeeSchedule: any[] = [];
  currentWeekStart: Date = this.getMonday(new Date());

  selectedMonth = new Date().getMonth();
  selectedWeek = this.getWeekNumber(new Date());

  months = [
    { name: 'Januar', value: 0 },
    { name: 'Februar', value: 1 },
    { name: 'Marts', value: 2 },
    { name: 'April', value: 3 },
    { name: 'Maj', value: 4 },
    { name: 'Juni', value: 5 },
    { name: 'Juli', value: 6 },
    { name: 'August', value: 7 },
    { name: 'September', value: 8 },
    { name: 'Oktober', value: 9 },
    { name: 'November', value: 10 },
    { name: 'December', value: 11 }
  ];

  weeks = Array.from({ length: 53 }, (_, i) => i + 1);

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.loadRoles();
    this.loadEmployeesAndShifts();
  }

  openShiftRow: any = {
    employeeName: 'Ledige vagter',
    monday: null,
    tuesday: null,
    wednesday: null,
    thursday: null,
    friday: null
  };

  goToSelectedWeek() {
    const year = new Date().getFullYear();

    const firstDayOfYear = new Date(year, 0, 1);
    const daysToAdd = (this.selectedWeek - 1) * 7;

    const targetDate = new Date(firstDayOfYear);
    targetDate.setDate(firstDayOfYear.getDate() + daysToAdd);

    this.currentWeekStart = this.getMonday(targetDate);
    this.buildEmployeeSchedule();
  }

  getMonday(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1);
    return new Date(d.setDate(diff));
  }

  getWeekNumber(date: Date): number {

    const d = new Date(date);

    d.setHours(0, 0, 0, 0);

    d.setDate(d.getDate() + 4 - (d.getDay() || 7));

    const yearStart = new Date(d.getFullYear(), 0, 1);

    return Math.ceil((((d.getTime() - yearStart.getTime()) / 86400000) + 1) / 7);
  }

  getWeekText(): string {

    const weekNumber = this.getWeekNumber(this.currentWeekStart);

    const startDate = new Date(this.currentWeekStart);

    const endDate = new Date(this.currentWeekStart);
    endDate.setDate(endDate.getDate() + 6);

    const startText = startDate.toLocaleDateString('da-DK');
    const endText = endDate.toLocaleDateString('da-DK');

    return `Uge ${weekNumber} (${startText} - ${endText})`;
  }

  goToPreviousWeek() {
    this.currentWeekStart.setDate(this.currentWeekStart.getDate() - 7);
    this.buildEmployeeSchedule();
  }

  goToNextWeek() {
    this.currentWeekStart.setDate(this.currentWeekStart.getDate() + 7);
    this.buildEmployeeSchedule();
  }

  goToToday() {
    this.currentWeekStart = this.getMonday(new Date());
    this.selectedWeek = this.getWeekNumber(this.currentWeekStart);
    this.selectedMonth = this.currentWeekStart.getMonth();
    this.buildEmployeeSchedule();
  }

  goToSelectedMonth() {
    const year = new Date().getFullYear();
    const date = new Date(year, this.selectedMonth, 1);

    this.currentWeekStart = this.getMonday(date);
    this.selectedWeek = this.getWeekNumber(this.currentWeekStart);

    this.buildEmployeeSchedule();
  }

  isShiftInCurrentWeek(shift: any): boolean {
    const shiftDate = new Date(shift.startTime);

    const weekEnd = new Date(this.currentWeekStart);
    weekEnd.setDate(weekEnd.getDate() + 7);

    return shiftDate >= this.currentWeekStart && shiftDate < weekEnd;
  }

  loadShifts() {
    this.http.get<any[]>('http://localhost:5000/api/shifts/')
      .subscribe({
        next: data => {
          this.shifts = data;

          this.buildEmployeeSchedule();

          this.cdr.detectChanges();
        },
        error: error => {
          console.error(error);
        }
      });
  }

  loadEmployeesAndShifts() {
    this.http.get<any[]>('http://localhost:5000/api/employees/')
      .subscribe({
        next: employees => {

          this.employees = employees;
          this.loadEmployeeRoles();

          setTimeout(() => {
            this.loadShifts();
          }, 500);
        },
        error: error => {
          console.error(error);
        }
      });
  }

  loadEmployeeRoles() {
    this.employeeRoles = [];

    this.employees.forEach(employee => {
      this.http.get<any[]>(
        `http://localhost:5000/api/employee-roles/employee/${employee.employeeId}`
      )
        .subscribe({
          next: roles => {
            this.employeeRoles.push(...roles);
          },
          error: error => {
            console.error(error);
          }
        });
    });
  }

  loadRoles() {
    this.http.get<any[]>('http://localhost:5000/api/roles/')
      .subscribe({
        next: data => this.roles = data,
        error: error => console.error(error)
      });
  }

  formatTime(dateTime: string): string {
    return new Date(dateTime).toLocaleTimeString('da-DK', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatShiftTime(shift: any): string {
    return `${this.formatTime(shift.startTime)} - ${this.formatTime(shift.endTime)}`;
  }

  getShiftTextForDay(dayIndex: number): string {
    const shiftsForDay = this.shifts.filter(shift => {
      const date = new Date(shift.startTime);
      return date.getDay() === dayIndex;
    });

    return shiftsForDay
      .map(shift => this.formatShiftTime(shift))
      .join(', ');
  }

  buildEmployeeSchedule() {
    const groupedByEmployee: any = {};

    this.openShiftRow = {
      employeeName: 'Ledige vagter',
      monday: null,
      tuesday: null,
      wednesday: null,
      thursday: null,
      friday: null
    };

    this.shifts
      .filter(shift => this.isShiftInCurrentWeek(shift))
      .forEach(shift => {

        if (!shift.isAssigned) {

          const day = new Date(shift.startTime).getDay();
          const shiftText = this.formatShiftTime(shift);

          if (day === 1) {
            this.openShiftRow.monday = {
              text: shiftText,
              color: '#d9d9d9',
              role: this.getRoleName(shift.roleId)
            };
          }

          if (day === 2) {
            this.openShiftRow.tuesday = {
              text: shiftText,
              color: '#d9d9d9',
              role: this.getRoleName(shift.roleId)
            };
          }

          if (day === 3) {
            this.openShiftRow.wednesday = {
              text: shiftText,
              color: '#d9d9d9',
              role: this.getRoleName(shift.roleId)
            };
          }

          if (day === 4) {
            this.openShiftRow.thursday = {
              text: shiftText,
              color: '#d9d9d9',
              role: this.getRoleName(shift.roleId)
            };
          }

          if (day === 5) {
            this.openShiftRow.friday = {
              text: shiftText,
              color: '#d9d9d9',
              role: this.getRoleName(shift.roleId)
            };
          }

          return;
        }

        const employeeName = this.getEmployeeNameFromEmployeeRoleId(shift.employeeRoleId);

        if (!groupedByEmployee[employeeName]) {
          groupedByEmployee[employeeName] = {
            employeeName: employeeName,
            monday: null,
            tuesday: null,
            wednesday: null,
            thursday: null,
            friday: null
          };
        }

        const day = new Date(shift.startTime).getDay();
        const shiftText = this.formatShiftTime(shift);

        if (day === 1) {
          groupedByEmployee[employeeName].monday = {
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          };
        }

        if (day === 2) {
          groupedByEmployee[employeeName].tuesday = {
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          };
        }

        if (day === 3) {
          groupedByEmployee[employeeName].wednesday = {
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          };
        }

        if (day === 4) {
          groupedByEmployee[employeeName].thursday = {
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          };
        }

        if (day === 5) {
          groupedByEmployee[employeeName].friday = {
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          };
        }

      });

    this.employeeSchedule = Object.values(groupedByEmployee);
  }

  getRoleColor(roleId: string): string {
    const role = this.roles.find(r => r.roleId === roleId);

    if (!role) {
      return '#cccccc';
    }

    switch (role.name) {
      case 'Kok':
        return '#dfe8f6';

      case 'Tjener':
        return '#eadcf5';

      case 'Opvasker':
        return '#f8efd4';

      default:
        return '#6f83b8';
    }
  }

  getRoleName(roleId: string): string {

    const role = this.roles.find(r => r.roleId === roleId);

    return role ? role.name : '';
  }

  getEmployeeNameFromEmployeeRoleId(employeeRoleId: string): string {
    const employeeRole = this.employeeRoles.find(er =>
      er.employeeRoleId === employeeRoleId
    );

    if (!employeeRole) {
      return 'Ukendt medarbejder';
    }

    const employee = this.employees.find(emp =>
      emp.employeeId === employeeRole.employeeId
    );

    if (!employee) {
      return 'Ukendt medarbejder';
    }

    return `${employee.firstName} ${employee.lastName}`;
  }

}

