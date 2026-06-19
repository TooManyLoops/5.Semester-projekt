import { ChangeDetectorRef, Component } from '@angular/core';

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar {
  showShiftMenu = false;

  userRole = localStorage.getItem('role');

  constructor(private cdr: ChangeDetectorRef) { }

  toggleShiftMenu(event?: MouseEvent) {
    event?.stopPropagation();
    this.showShiftMenu = !this.showShiftMenu;
    this.cdr.detectChanges();
  }

  logout() {
    localStorage.removeItem('role');
    window.location.href = '/login';
  }
}
