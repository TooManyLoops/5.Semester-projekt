import { Component } from '@angular/core';

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar {
  showShiftMenu = false;

  toggleShiftMenu() {
    this.showShiftMenu = !this.showShiftMenu;
  }
}