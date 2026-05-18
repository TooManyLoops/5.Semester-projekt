import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-employees',
  standalone: false,
  templateUrl: './employees.html',
  styleUrl: './employees.css',
})
export class Employees implements OnInit {

  employees: any[] = [];

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadEmployees();
  }

  loadEmployees() {
    this.http.get<any[]>('http://localhost:5000/api/employees/')
      .subscribe({
        next: data => {
          this.employees = data;
          this.cdr.detectChanges();
        },
        error: error => {
          console.error(error);
        }
      });
  }

  getStatusText(status: number): string {

    switch (status) {

      case 1:
        return 'Active';

      case 2:
        return 'Inactive';

      case 3:
        return 'Terminated';

      case 4:
        return 'On Leave';

      default:
        return 'Unknown';
    }
  }

}
