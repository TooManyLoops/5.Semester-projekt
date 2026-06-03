import { Component, OnInit } from '@angular/core';
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
  employees: any[] = [];
  times: string[] = [];
  today = '';
  employeeRoles: any[] = [];
  filteredEmployees: any[] = [];

  shift = {
    date: '',
    startTime: '',
    endTime: '',
    roleId: '',
    employeeId: ''
  };

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.generateTimes();
    this.today = new Date().toISOString().split('T')[0];
  }

  ngOnInit() {
    this.loadRoles();
    this.loadEmployees();
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

  loadRoles() {
    this.http.get<any[]>('http://localhost:5000/api/roles/')
      .subscribe({
        next: data => this.roles = data,
        error: error => console.error(error)
      });
  }

  loadEmployees() {
    this.http.get<any[]>('http://localhost:5000/api/employees/')
      .subscribe({
        next: data => {
          this.employees = data;
          this.filteredEmployees = data;

          this.employees.forEach(employee => {
            this.http.get<any[]>(
              `http://localhost:5000/api/employee-roles/employee/${employee.employeeId}`
            )
              .subscribe({
                next: roles => {
                  this.employeeRoles.push(...roles);
                },
                error: error => console.error(error)
              });
          });
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
    const request = {
      startTime: `${this.shift.date}T${this.shift.startTime}:00`,
      endTime: `${this.shift.date}T${this.shift.endTime}:00`,
      shiftRequirements: [
        {
          roleId: this.shift.roleId,
          amount: 1
        }
      ]
    };

    console.log('REQUEST SENDT:', request);

    this.http.post('http://localhost:5000/api/shifts/', request)
      .subscribe({
        next: createdShift => {
          if (!this.shift.employeeId) {
            alert('Vagt oprettet som ledig vagt');
            this.router.navigate(['/shifts/open']);
            return;
          }

          this.http.get<any[]>(
            `http://localhost:5000/api/employee-roles/employee/${this.shift.employeeId}`
          )
            .subscribe({
              next: employeeRoles => {
                const matchingEmployeeRole = employeeRoles.find(er =>
                  er.roleId === this.shift.roleId
                );

                if (!matchingEmployeeRole) {
                  alert('Medarbejderen har ikke den valgte rolle');
                  return;
                }

                console.log('MATCHING EMPLOYEE ROLE:', matchingEmployeeRole);

                const assignmentRequest = {
                  shiftId: (createdShift as any).shiftId,
                  employeeRoleId: matchingEmployeeRole.employeeRoleId,
                  assignmentStatus: 1
                };

                console.log('ASSIGN REQUEST:', assignmentRequest);

                this.http.post('http://localhost:5000/api/shifts/Assign', assignmentRequest)
                  .subscribe({
                    next: () => {
                      alert('Vagt oprettet og tildelt medarbejder');
                      this.router.navigate(['/shifts']);
                    },

                    error: error => {
                      console.error(error);

                      alert(
                        'Assign fejlede\n' +
                        'Status: ' + error.status + '\n' +
                        'Body: ' + JSON.stringify(error.error)
                      );
                    }
                  });
              },
              error: error => {
                console.error(error);
                alert('Kunne ikke hente medarbejderens roller');
              }
            });
        },

        error: error => {
          console.error(error);
          console.log('ERROR STATUS:', error.status);
          console.log('ERROR BODY:', error.error);
          console.log('DATA SENDT:', request);
          alert('Fejl ved oprettelse af vagt');
        }
      });
  }
}
