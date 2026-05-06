import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App {
  employee = {
    firstName: '',
    lastName: '',
    email: '',
    phone: ''
  };

  createEmployee() {
    alert('Save blev klikket');
    console.log(this.employee);
  }
}
