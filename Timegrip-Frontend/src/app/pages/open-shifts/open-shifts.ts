import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-open-shifts',
  standalone: false,
  templateUrl: './open-shifts.html',
  styleUrl: './open-shifts.css'
})
export class OpenShifts implements OnInit {

  openShifts: any[] = [];
  roles: any[] = [];
  employees: any[] = [];
  selectedShift: any = null;
  selectedEmployeeId = ''

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadEmployees();
    this.loadRolesAndShifts();
  }

  loadEmployees() {
    this.http.get<any[]>('http://localhost:5000/api/employees/')
      .subscribe({
        next: data => {
          this.employees = data;
        },
        error: error => {
          console.error(error);
        }
      });
  }

  loadRolesAndShifts() {
    this.http.get<any[]>('http://localhost:5000/api/roles/')
      .subscribe({
        next: roles => {
          this.roles = roles;
          this.loadOpenShifts();
        },
        error: error => {
          console.error(error);
        }
      });
  }

  loadOpenShifts() {
    this.http.get<any[]>('http://localhost:5000/api/shifts/')
      .subscribe({
        next: data => {
          this.openShifts = data
            .filter(shift => !shift.isAssigned)
            .map(shift => ({
              ...shift,
              roleName: this.getRoleName(shift)
            }));

          this.cdr.detectChanges();
        },
        error: error => {
          console.error(error);
        }
      });
  }

  joinShift(shift: any) {

    const employeeId = prompt(
      'Indtast medarbejderens ID'
    );

    if (!employeeId) {
      return;
    }

    this.http.get<any[]>(
      `http://localhost:5000/api/employee-roles/employee/${employeeId}`
    )
      .subscribe({
        next: employeeRoles => {

          const matchingEmployeeRole = employeeRoles.find(er =>
            er.roleId === shift.roleId
          );

          if (!matchingEmployeeRole) {
            alert('Medarbejderen har ikke den nødvendige rolle');
            return;
          }

          const assignmentRequest = {
            shiftId: shift.shiftId,
            employeeRoleId: matchingEmployeeRole.employeeRoleId,
            assignmentStatus: 1
          };

          this.http.post(
            'http://localhost:5000/api/shifts/Assign',
            assignmentRequest
          )
            .subscribe({
              next: () => {
                alert('Vagt tildelt');
                this.loadOpenShifts();
              },
              error: error => {
                console.error(error);
                alert('Kunne ikke tildele vagten');
              }
            });
        },
        error: error => {
          console.error(error);
        }
      });
  }

  confirmJoinShift() {

    if (!this.selectedEmployeeId || !this.selectedShift) {
      return;
    }

    this.http.get<any[]>(
      `http://localhost:5000/api/employee-roles/employee/${this.selectedEmployeeId}`
    )
      .subscribe({
        next: employeeRoles => {

          const matchingEmployeeRole = employeeRoles.find(er =>
            er.roleId === this.selectedShift.roleId
          );

          if (!matchingEmployeeRole) {
            alert('Medarbejderen har ikke den nødvendige rolle');
            return;
          }

          const assignmentRequest = {
            shiftId: this.selectedShift.shiftId,
            employeeRoleId: matchingEmployeeRole.employeeRoleId,
            assignmentStatus: 1
          };

          this.http.post(
            'http://localhost:5000/api/shifts/Assign',
            assignmentRequest
          )
            .subscribe({
              next: () => {
                alert('Vagt tildelt');

                this.selectedShift = null;
                this.selectedEmployeeId = '';

                this.loadOpenShifts();
              },
              error: error => {
                console.error(error);
                alert('Kunne ikke tildele vagten');
              }
            });
        },
        error: error => {
          console.error(error);
        }
      });
  }

  getRoleName(shift: any): string {

    const roleId = shift.roleId;

    if (!roleId) {
      return 'Ingen rolle';
    }

    const role = this.roles.find(r => r.roleId === roleId);

    return role ? role.name : 'Ukendt rolle';
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
}