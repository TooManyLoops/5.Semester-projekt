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

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadRolesAndShifts();
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
            .filter(shift =>
              !shift.shiftAssignments || shift.shiftAssignments.length === 0
            )
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