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
    console.log('SHIFTS COMPONENT LOADED');
    this.loadEmployeesAndShifts();
  }

  loadShifts() {
    this.http.get<any[]>('http://localhost:5000/api/shifts/')
      .subscribe({
        next: data => {
          this.shifts = data.filter(shift => shift.isAssigned === true);

          this.employeeSchedule = [
            {
              employeeName: this.getEmployeeNameFromEmployeeRoleId(this.shifts[0]?.employeeRoleId),
              color: '#96768f',

              monday: this.getShiftTextForDay(1),
              tuesday: this.getShiftTextForDay(2),
              wednesday: this.getShiftTextForDay(3),
              thursday: this.getShiftTextForDay(4),
              friday: this.getShiftTextForDay(5)
            }
          ];

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
          console.log('EMPLOYEES:', employees);

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

  formatDate(dateTime: string): string {
    return new Date(dateTime).toLocaleDateString('da-DK');
  }

  getShiftsForDay(dayIndex: number) {
    return this.shifts.filter(shift => {
      const date = new Date(shift.startTime);
      return date.getDay() === dayIndex;
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

