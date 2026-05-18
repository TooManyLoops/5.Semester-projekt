import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-edit-employee',
  standalone: false,
  templateUrl: './edit-employee.html',
  styleUrl: './edit-employee.css'
})
export class EditEmployee implements OnInit {

  employeeId: string | null = null;

  employee = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    employeeStatus: 1
  };

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {

    this.employeeId = this.route.snapshot.paramMap.get('id');

    this.http.get<any>(`http://localhost:5000/api/employees/${this.employeeId}`)
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