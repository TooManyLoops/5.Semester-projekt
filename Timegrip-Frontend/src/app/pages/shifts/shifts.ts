import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-shifts',
  standalone: false,
  templateUrl: './shifts.html',
  styleUrl: './shifts.css'
})
export class Shifts implements OnInit {

  shifts: any[] = [];

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.loadShifts();
  }

  loadShifts() {
    this.http.get<any[]>('http://localhost:5000/api/aggregate/shifts')
      .subscribe({
        next: data => {
          console.log('SHIFTS FRA DB:', data);
          this.shifts = data;
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

  getShiftsForDay(dayIndex: number) {
    return this.shifts.filter(shift => {
      const date = new Date(shift.startTime);
      return date.getDay() === dayIndex;
    });
  }

  formatShiftTime(shift: any): string {
    return `${this.formatTime(shift.startTime)} - ${this.formatTime(shift.endTime)}`;
  }

  employeeSchedule = [
    {
      employeeName: 'Khanh Do',
      color: '#96768f',

      monday: '08:00 - 16:00',
      tuesday: '',
      wednesday: '12:00 - 20:00',
      thursday: '',
      friday: ''
    },

    {
      employeeName: 'Maria Jensen',
      color: '#5b8c85',

      monday: '',
      tuesday: '10:00 - 18:00',
      wednesday: '',
      thursday: '',
      friday: '08:00 - 14:00'
    }
  ];
}

