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
    this.router.navigate(['/shifts']);
  }

  loginAsEmployee() {

    localStorage.setItem('role', 'employee');
    this.router.navigate(['/shifts']);
  }
}