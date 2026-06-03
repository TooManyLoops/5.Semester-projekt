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

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.loadRoles();
    this.loadEmployeesAndShifts();
  }

  loadShifts() {
    this.http.get<any[]>('http://localhost:5000/api/shifts/')
      .subscribe({
        next: data => {
          this.shifts = data.filter(shift => shift.isAssigned === true);

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

    this.shifts.forEach(shift => {
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
          color: this.getRoleColor(shift.roleId)
        };
      }

      if (day === 2) {
        groupedByEmployee[employeeName].tuesday = {
          text: shiftText,
          color: this.getRoleColor(shift.roleId)
        };
      }

      if (day === 3) {
        groupedByEmployee[employeeName].wednesday = {
          text: shiftText,
          color: this.getRoleColor(shift.roleId)
        };
      }

      if (day === 4) {
        groupedByEmployee[employeeName].thursday = {
          text: shiftText,
          color: this.getRoleColor(shift.roleId)
        };
      }

      if (day === 5) {
        groupedByEmployee[employeeName].friday = {
          text: shiftText,
          color: this.getRoleColor(shift.roleId)
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
        return '#96768f';

      case 'Tjener':
        return '#5b8c85';

      case 'Opvasker':
        return '#c98b5f';

      default:
        return '#6f83b8';
    }
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

  employeeSchedule: any[] = [];
}

