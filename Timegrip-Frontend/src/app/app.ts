import { Component } from '@angular/core';
import { Header } from './header/header';
import { RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';


@Component({
  selector: 'app-root',
  imports: [
    Header,
    RouterOutlet,
    FormsModule,
    HttpClientModule
  ],
  standalone: true,
  templateUrl: './app.html',
  styleUrl: './app.css'

})
export class App {
  employee = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    employeeStatus: 1
  };

  constructor(private http: HttpClient) { }

  addEmployee() {

    alert('addEmployee bliver kaldt');

    this.http.post('http://localhost:5208/Employees/', this.employee)
      .subscribe(
        {
          next: response => {
            alert('Employee oprettet');
            console.log(response);
          },
          error: error => {
            alert('Fejl ved oprettelse');
            console.error(error);
            console.log('STATUS:', error.status);
            console.log('VALIDATION ERROR:', error.error);
            console.log('DATA SENDT:', this.employee);
          }

        });
  }
}


