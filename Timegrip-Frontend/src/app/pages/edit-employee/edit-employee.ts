import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeForm, EmployeeRole, Role } from '../../model';

@Component({
  selector: 'app-edit-employee',
  standalone: false,
  templateUrl: './edit-employee.html',
  styleUrl: './edit-employee.css'
})
export class EditEmployee implements OnInit {

  employeeId: string | null = null;

  employee: EmployeeForm = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    employeeStatus: 1
  };

  roles: Role[] = [];
  employeeRoles: EmployeeRole[] = [];
  selectedRoleId = '';

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.employeeId = this.route.snapshot.paramMap.get('id');

    this.loadEmployee();
    this.loadRoles();
    this.loadEmployeeRoles();
  }

  loadEmployee() {
    this.http.get<EmployeeForm>(`http://localhost:5000/api/employees/${this.employeeId}`)
      .subscribe({
        next: data => {
          this.employee = data;
          this.cdr.detectChanges();
        },
        error: error => {
          console.error(error);
        }
      });
  }

  loadRoles() {
    this.http.get<Role[]>('http://localhost:5000/api/roles/')
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

  loadEmployeeRoles() {
    this.http.get<EmployeeRole[]>(
      `http://localhost:5000/api/employee-roles/employee/${this.employeeId}`
    )
      .subscribe({
        next: data => {
          this.employeeRoles = data;
          this.cdr.detectChanges();
        },
        error: error => {
          console.error(error);
        }
      });
  }

  addRoleToEmployee() {
    if (!this.selectedRoleId) {
      alert('Vælg en rolle');
      return;
    }

    const request = {
      employeeId: this.employeeId,
      roleId: this.selectedRoleId,
      isPrimary: false
    };

    this.http.post('http://localhost:5000/api/employee-roles/', request)
      .subscribe({
        next: () => {
          alert('Rolle tilføjet');
          this.selectedRoleId = '';
          this.loadEmployeeRoles();
        },
        error: error => {
          console.error(error);
        }
      });
  }

  getRoleName(roleId: string): string {
    const role = this.roles.find(r => r.roleId === roleId);
    return role ? role.name : 'Ukendt rolle';
  }

  updateEmployee() {
    this.http.put(`http://localhost:5000/api/employees/${this.employeeId}`, this.employee)
      .subscribe({
        next: response => {
          alert('Employee updated');
          this.router.navigate(['/employees']);
        },
        error: error => {
          console.error(error);
        }
      });
  }
}
