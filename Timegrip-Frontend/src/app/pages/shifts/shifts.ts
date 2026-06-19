import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

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
  selectedEmployeeId = '';
  selectedEmployeeForShift = '';
  showAllShifts = false;
  selectedDayShifts: any[] = [];
  selectedDayName = '';
  isAssigningShift = false;
  loggedInRole = localStorage.getItem('role');
  loggedInEmployeeId = localStorage.getItem('employeeId');
  isMySchedulePage = false;


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
    private cdr: ChangeDetectorRef,
    private router: Router
  ) { }

  ngOnInit() {
    this.isMySchedulePage = this.router.url === '/my-shifts';

    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.isMySchedulePage = this.router.url === '/my-shifts';

        if (this.shifts.length > 0) {
          this.buildEmployeeSchedule();
          this.cdr.detectChanges();
        }
      });

    this.loadEmployeesAndShifts();
  }

  openShiftRow: any = {
    employeeName: 'Ledige vagter',
    monday: [],
    tuesday: [],
    wednesday: [],
    thursday: [],
    friday: [],
    saturday: [],
    sunday: []
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
    d.setHours(0, 0, 0, 0);

    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1);

    d.setDate(diff);
    return d;
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
    this.http.get<any[]>('http://localhost:5000/api/aggregate/shifts')
      .subscribe({
        next: data => {
          this.shifts = data.flatMap(shift => this.normalizeShift(shift));
          this.buildEmployeeSchedule();
          this.cdr.detectChanges();
        },
        error: error => {
          console.error(error);
        }
      });
  }

  loadEmployeesAndShifts() {
    this.http.get<any>('http://localhost:5000/api/aggregate/forms/create-shift')
      .subscribe({
        next: data => {
          this.employees = data.employees;
          this.roles = data.roles;
          this.employeeRoles = this.employees.flatMap(employee =>
            (employee.roles ?? []).map((role: any) => ({
              employeeId: employee.employeeId,
              employeeRoleId: role.employeeRoleId,
              roleId: role.roleId,
              roleName: role.name,
              isPrimary: role.isPrimary
            }))
          );
          this.cdr.detectChanges();
          this.loadShifts();
        },
        error: error => {
          console.error(error);
        }
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
      monday: [],
      tuesday: [],
      wednesday: [],
      thursday: [],
      friday: [],
      saturday: [],
      sunday: []
    };

    this.shifts
      .filter(shift => this.isShiftInCurrentWeek(shift))
      .forEach(shift => {

        if (!shift.isAssigned) {
          if (this.isMySchedulePage) {
            return;
          }

          if (this.loggedInRole === 'employee') {

            const employeeHasRole = this.employeeRoles.some(er =>

              er.employeeId === this.loggedInEmployeeId &&

              er.roleId === shift.roleId

            );

            if (!employeeHasRole) {

              return;

            }

          }

          const day = new Date(shift.startTime).getDay();
          const shiftText = this.formatShiftTime(shift);

          if (day === 1) {
            this.openShiftRow.monday.push({
              shiftId: shift.shiftId,
              roleId: shift.roleId,
              text: shiftText,
              color: this.getRoleColor(shift.roleId),
              role: this.getRoleName(shift.roleId)
            });
          }

          if (day === 2) {
            this.openShiftRow.tuesday.push({
              shiftId: shift.shiftId,
              roleId: shift.roleId,
              text: shiftText,
              color: this.getRoleColor(shift.roleId),
              role: this.getRoleName(shift.roleId)
            });
          }

          if (day === 3) {
            this.openShiftRow.wednesday.push({
              shiftId: shift.shiftId,
              roleId: shift.roleId,
              text: shiftText,
              color: this.getRoleColor(shift.roleId),
              role: this.getRoleName(shift.roleId)
            });
          }

          if (day === 4) {
            this.openShiftRow.thursday.push({
              shiftId: shift.shiftId,
              roleId: shift.roleId,
              text: shiftText,
              color: this.getRoleColor(shift.roleId),
              role: this.getRoleName(shift.roleId)
            });
          }

          if (day === 5) {
            this.openShiftRow.friday.push({
              shiftId: shift.shiftId,
              roleId: shift.roleId,
              text: shiftText,
              color: this.getRoleColor(shift.roleId),
              role: this.getRoleName(shift.roleId)
            });
          }

          if (day === 6) {
            this.openShiftRow.saturday.push({
              shiftId: shift.shiftId,
              roleId: shift.roleId,
              text: shiftText,
              color: this.getRoleColor(shift.roleId),
              role: this.getRoleName(shift.roleId)
            });
          }

          if (day === 0) {
            this.openShiftRow.sunday.push({
              shiftId: shift.shiftId,
              roleId: shift.roleId,
              text: shiftText,
              color: this.getRoleColor(shift.roleId),
              role: this.getRoleName(shift.roleId)
            });
          }

          return;
        }

        const employeeRole = this.employeeRoles.find(er =>
          er.employeeRoleId === shift.employeeRoleId
        );

        if (!employeeRole) {
          return;
        }

        if (
          this.selectedEmployeeId &&
          employeeRole.employeeId !== this.selectedEmployeeId
        ) {
          return;
        }

        if (
          this.isMySchedulePage &&
          employeeRole.employeeId !== this.loggedInEmployeeId
        ) {
          return;
        }

        const employeeName = this.getEmployeeNameFromEmployeeRoleId(shift.employeeRoleId);

        if (!groupedByEmployee[employeeName]) {
          groupedByEmployee[employeeName] = {
            employeeName: employeeName,
            monday: [],
            tuesday: [],
            wednesday: [],
            thursday: [],
            friday: [],
            saturday: [],
            sunday: []
          };
        }

        const day = new Date(shift.startTime).getDay();
        const shiftText = this.formatShiftTime(shift);

        if (day === 1) {
          groupedByEmployee[employeeName].monday.push({
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          });
        }

        if (day === 2) {
          groupedByEmployee[employeeName].tuesday.push({
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          });
        }

        if (day === 3) {
          groupedByEmployee[employeeName].wednesday.push({
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          });
        }

        if (day === 4) {
          groupedByEmployee[employeeName].thursday.push({
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          });
        }

        if (day === 5) {
          groupedByEmployee[employeeName].friday.push({
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          });
        }

        if (day === 6) {
          groupedByEmployee[employeeName].saturday.push({
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          });
        }

        if (day === 0) {
          groupedByEmployee[employeeName].sunday.push({
            text: shiftText,
            color: this.getRoleColor(shift.roleId),
            role: this.getRoleName(shift.roleId)
          });
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

  selectedOpenShift: any = null;

  selectOpenShift(shift: any) {
    this.selectedOpenShift = shift;
    this.selectedEmployeeForShift = '';

    this.showAllShifts = false;
    this.selectedDayShifts = [];
    this.selectedDayName = '';

    console.log('Valgt ledig vagt:', shift);
  }

  assignSelectedShift() {

    const matchingEmployeeRole = this.employeeRoles.find(er =>
      er.employeeId === this.selectedEmployeeForShift &&
      er.roleId === this.selectedOpenShift.roleId
    );

    if (!matchingEmployeeRole) {
      alert('Medarbejderen har ikke den nødvendige rolle');
      return;
    }

    const hasConflict = this.shifts.some(shift => {
      if (!shift.isAssigned) {
        return false;
      }

      const existingEmployeeRole = this.employeeRoles.find(er =>
        er.employeeRoleId === shift.employeeRoleId
      );

      if (!existingEmployeeRole) {
        return false;
      }

      if (
        existingEmployeeRole.employeeId?.toLowerCase() !==
        this.selectedEmployeeForShift?.toLowerCase()
      ) {
        return false;
      }

      const existingStart = new Date(shift.startTime);
      const existingEnd = new Date(shift.endTime);

      const newShift = this.shifts.find(s =>
        s.shiftId === this.selectedOpenShift.shiftId
      );

      if (!newShift) {
        return false;
      }

      const newStart = new Date(newShift.startTime);
      const newEnd = new Date(newShift.endTime);

      return newStart < existingEnd && newEnd > existingStart;
    });

    if (hasConflict) {
      alert('Medarbejderen har allerede en vagt i dette tidsrum');
      return;
    }

    const assignmentRequest = {
      employeeRoleId: matchingEmployeeRole.employeeRoleId,
      assignmentStatus: 1
    };

    this.http.post(
      `http://localhost:5000/api/aggregate/shifts/${this.selectedOpenShift.shiftId}/assign`,
      assignmentRequest
    )
      .subscribe({
        next: () => {
          this.selectedOpenShift = null;
          this.selectedEmployeeForShift = '';
          this.loadShifts();

          setTimeout(() => {
            this.cdr.detectChanges();
          }, 100);
        },
        error: error => {
          console.error(error);
          alert('Kunne ikke tildele vagten');
        }
      });


  }

  getEmployeesForSelectedOpenShift(): any[] {
    if (!this.selectedOpenShift) {
      return [];
    }

    const employeeIdsWithRole = this.employeeRoles
      .filter(er => er.roleId === this.selectedOpenShift.roleId)
      .map(er => er.employeeId);

    return this.employees.filter(employee =>
      employeeIdsWithRole.includes(employee.employeeId)
    );
  }

  getDayHeader(dayOffset: number): string {
    const date = new Date(this.currentWeekStart);
    date.setDate(date.getDate() + dayOffset);

    const dayNames = ['Mandag', 'Tirsdag', 'Onsdag', 'Torsdag', 'Fredag', 'Lørdag', 'Søndag'];
    const dateText = date.toLocaleDateString('da-DK', {
      day: 'numeric',
      month: 'numeric'
    });

    return `${dayNames[dayOffset]} ${dateText}`;
  }

  showMoreShifts(shifts: any[], dayName: string) {

    this.selectedDayShifts = shifts;
    this.selectedDayName = dayName;
    this.showAllShifts = true;

  }

  signUpForShift() {

    this.selectedEmployeeForShift = this.loggedInEmployeeId || '';

    this.assignSelectedShift();

  }

  private normalizeShift(shift: any): any[] {
    if (!shift.shiftRequirements && !shift.shiftAssignments) {
      return [shift];
    }

    const assignments = shift.shiftAssignments ?? [];
    const requirements = shift.shiftRequirements ?? [];

    if (assignments.length === 0) {
      return requirements.flatMap((requirement: any) =>
        Array.from({ length: requirement.amount || 1 }, () => ({
          shiftId: shift.shiftId,
          startTime: shift.startTime,
          endTime: shift.endTime,
          isAssigned: false,
          roleId: requirement.roleId,
          roleName: requirement.roleName
        }))
      );
    }

    return assignments.map((assignment: any) => {
      const employeeRole = this.employeeRoles.find(er =>
        er.employeeRoleId === assignment.employeeRoleId
      );
      const requirement = requirements.find((item: any) =>
        item.roleId === employeeRole?.roleId
      ) ?? requirements[0];

      return {
        shiftId: shift.shiftId,
        startTime: shift.startTime,
        endTime: shift.endTime,
        isAssigned: true,
        employeeRoleId: assignment.employeeRoleId,
        roleId: employeeRole?.roleId ?? requirement?.roleId,
        roleName: requirement?.roleName
      };
    });
  }

}

