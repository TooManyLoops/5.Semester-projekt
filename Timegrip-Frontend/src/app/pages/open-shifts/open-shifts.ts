import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-open-shifts',
  standalone: false,
  templateUrl: './open-shifts.html',
  styleUrl: './open-shifts.css'
})
export class OpenShifts implements OnInit {

  openShifts: any[] = [];

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadOpenShifts();
  }

  loadOpenShifts() {
    this.http.get<any[]>('http://localhost:5000/api/shifts/')
      .subscribe({
        next: data => {
          console.log('LEDIGE VAGTER:', data);

          this.openShifts = data;

          this.cdr.detectChanges();
        },
        error: error => {
          console.error(error);
        }
      });
  }

  formatTime(dateTime: string): string {
    return new Date(dateTime).toLocaleTimeString('da-DK', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatDate(dateTime: string): string {
    return new Date(dateTime).toLocaleDateString('da-DK');
  }
}
