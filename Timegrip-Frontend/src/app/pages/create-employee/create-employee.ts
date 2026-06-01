import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-employee',
  standalone: false,
  templateUrl: './create-employee.html',
  styleUrl: './create-employee.css'
})
export class CreateEmployee implements OnInit {

  roles: any[] = [];
  selectedRoleId = '';

  employee = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    employeeStatus: 1
  };

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.loadRoles();
  }

  loadRoles() {
    this.http.get<any[]>('http://localhost:5000/api/roles/')
      .subscribe({
        next: data => {
          this.roles = data;
          this.cdr.detectChanges();
        },
        error: error => {
          console.error(error);
        }
      });
  }

  addEmployee() {
    this.http.post<any>('http://localhost:5000/api/employees/', this.employee)
      .subscribe({
        next: createdEmployee => {

          if (!this.selectedRoleId) {
            alert('Medarbejder oprettet');
            this.router.navigate(['/employees']);
            return;
          }

          const employeeRoleRequest = {
            employeeId: createdEmployee.employeeId,
            roleId: this.selectedRoleId,
            isPrimary: true
          };

          this.http.post('http://localhost:5000/api/employee-roles/', employeeRoleRequest)
            .subscribe({
              next: () => {
                alert('Medarbejder og rolle oprettet');
                this.router.navigate(['/employees']);
              },
              error: error => {
                console.error(error);
                alert('Medarbejder blev oprettet, men rolle fejlede');
              }
            });
        },
        error: error => {
          alert('Fejl ved oprettelse');
          console.error(error);
        }
      });
  }
}
