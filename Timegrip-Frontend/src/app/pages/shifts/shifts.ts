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

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
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
          color: this.getEmployeeColor(employeeName),
          monday: '',
          tuesday: '',
          wednesday: '',
          thursday: '',
          friday: ''
        };
      }

      const day = new Date(shift.startTime).getDay();
      const shiftText = this.formatShiftTime(shift);

      if (day === 1) {
        groupedByEmployee[employeeName].monday += groupedByEmployee[employeeName].monday
          ? ', ' + shiftText
          : shiftText;
      }

      if (day === 2) {
        groupedByEmployee[employeeName].tuesday += groupedByEmployee[employeeName].tuesday
          ? ', ' + shiftText
          : shiftText;
      }

      if (day === 3) {
        groupedByEmployee[employeeName].wednesday += groupedByEmployee[employeeName].wednesday
          ? ', ' + shiftText
          : shiftText;
      }

      if (day === 4) {
        groupedByEmployee[employeeName].thursday += groupedByEmployee[employeeName].thursday
          ? ', ' + shiftText
          : shiftText;
      }

      if (day === 5) {
        groupedByEmployee[employeeName].friday += groupedByEmployee[employeeName].friday
          ? ', ' + shiftText
          : shiftText;
      }
    });

    this.employeeSchedule = Object.values(groupedByEmployee);
  }

  getEmployeeColor(employeeName: string): string {
    const colors = [
      '#96768f',
      '#5b8c85',
      '#c98b5f',
      '#6f83b8',
      '#9b6f9f'
    ];

    let sum = 0;

    for (let i = 0; i < employeeName.length; i++) {
      sum += employeeName.charCodeAt(i);
    }

    return colors[sum % colors.length];
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

