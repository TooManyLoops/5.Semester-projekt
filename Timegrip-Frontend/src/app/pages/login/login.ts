import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  constructor(private router: Router) { }

  loginAsLeader() {

    localStorage.setItem('role', 'leader');
    localStorage.removeItem('employeeId');
    this.router.navigate(['/shifts']);
  }

  loginAsEmployee() {

    localStorage.setItem('role', 'employee');

    localStorage.setItem(
      'employeeId',
      '03328d8a-195c-49e5-8045-5ac71b7d7729'
    );

    this.router.navigate(['/shifts']);
  }
}