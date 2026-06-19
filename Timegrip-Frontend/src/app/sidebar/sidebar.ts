import { Component } from '@angular/core';

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar {
  showShiftMenu = false;

  userRole = localStorage.getItem('role');

  toggleShiftMenu() {
    this.showShiftMenu = !this.showShiftMenu;
  }

  logout() {
    localStorage.removeItem('role');
    window.location.href = '/login';
  }
}