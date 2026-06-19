import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-shift',
  standalone: false,
  templateUrl: './create-shift.html',
  styleUrl: './create-shift.css'
})
export class CreateShift implements OnInit {

  roles: any[] = [];
  roleRequirements: any[] = [];
  filteredEmployees: any[] = [];
  employees: any[] = [];
  times: string[] = [];
  today = '';
  employeeRoles: any[] = [];

  shift = {
    date: '',
    startTime: '',
    endTime: '',
    roleId: '',
    employeeId: ''
  };

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    this.generateTimes();
    this.today = new Date().toISOString().split('T')[0];
  }

  ngOnInit() {
    this.loadCreateShiftForm();
  }

  generateTimes() {
    for (let hour = 7; hour <= 17; hour++) {
      for (let minute = 0; minute < 60; minute += 15) {

        if (hour === 17 && minute > 0) {
          break;
        }

        const h = hour.toString().padStart(2, '0');
        const m = minute.toString().padStart(2, '0');

        this.times.push(`${h}:${m}`);
      }
    }
  }

  loadCreateShiftForm() {
    this.http.get<any>('http://localhost:5000/api/aggregate/forms/create-shift')
      .subscribe({
        next: data => {
          this.roles = data.roles;
          this.employees = data.employees;
          this.roleRequirements = this.roles.map(role => ({
            roleId: role.roleId,
            roleName: role.name,
            count: 0
          }));
          this.employeeRoles = this.employees.flatMap(employee =>
            (employee.roles ?? []).map((role: any) => ({
              employeeId: employee.employeeId,
              employeeRoleId: role.employeeRoleId,
              roleId: role.roleId,
              roleName: role.name,
              isPrimary: role.isPrimary
            }))
          );
        },
        error: error => console.error(error)
      });
  }

  filterEmployeesByRole() {
    if (!this.shift.roleId) {
      this.filteredEmployees = this.employees;
      return;
    }

    const employeeIdsWithRole = this.employeeRoles
      .filter(er => er.roleId === this.shift.roleId)
      .map(er => er.employeeId);

    this.filteredEmployees = this.employees.filter(employee =>
      employeeIdsWithRole.includes(employee.employeeId)
    );

    this.shift.employeeId = '';
  }

  addShift() {
    const selectedRoles = this.roleRequirements.filter(role => role.count > 0);

    if (selectedRoles.length === 0) {
      alert('Vælg mindst én rolle');
      return;
    }

    selectedRoles.forEach(role => {
      for (let i = 0; i < role.count; i++) {

        const request = {
          startTime: `${this.shift.date}T${this.shift.startTime}:00`,
          endTime: `${this.shift.date}T${this.shift.endTime}:00`,
          shiftRequirements: [
            {
              roleId: role.roleId,
              amount: 1
            }
          ]
        };

        this.http.post('http://localhost:5000/api/shifts/', request)
          .subscribe({
            next: response => {
              console.log('Vagt oprettet:', response);
            },
            error: error => {
              console.error(error);
              alert('Fejl ved oprettelse af vagt');
            }
          });
      }
    });

    alert('Vagter oprettet');
    this.router.navigate(['/shifts']);
  }
}
