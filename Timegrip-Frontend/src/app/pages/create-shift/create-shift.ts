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
        next: data => this.employees = data,
        error: error => console.error(error)
      });
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
        next: response => {
          alert('Vagt oprettet');
          console.log(response);
          this.router.navigate(['/shifts']);
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
