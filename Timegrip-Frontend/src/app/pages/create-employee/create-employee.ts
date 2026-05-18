import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-employee',
  standalone: false,
  templateUrl: './create-employee.html',
  styleUrl: './create-employee.css'
})
export class CreateEmployee {

  employee = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    employeeStatus: 1
  };

  constructor(
    private http: HttpClient,
    private router: Router
  ) { }

  addEmployee() {
    this.http.post('http://localhost:5000/api/employees/', this.employee)
      .subscribe({
        next: response => {
          alert('Employee oprettet');
          console.log(response);

          this.router.navigate(['/employees']);
        },
        error: error => {
          alert('Fejl ved oprettelse');
          console.error(error);
        }
      });
  }
}