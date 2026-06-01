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

          this.employees = data.map(emp => ({
            ...emp,
            rolesText: ''
          }));

          this.employees.forEach(emp => {
            this.http.get<any[]>(
              `http://localhost:5000/api/employee-roles/employee/${emp.employeeId}`
            )
              .subscribe({
                next: roles => {
                  emp.rolesText = roles
                    .map(role => role.roleName)
                    .join(', ');

                  this.cdr.detectChanges();
                },
                error: error => {
                  console.error(error);
                }
              });

          });

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

  currentPage = 1;
  pageSize = 10;
  selectedLetter = '';

  letters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ'.split('');

  get filteredEmployees() {
    let result = [...this.employees];

    if (this.selectedLetter) {
      result = result.filter(emp =>
        emp.firstName?.toUpperCase().startsWith(this.selectedLetter)
      );
    }

    return result.sort((a, b) =>
      a.firstName.localeCompare(b.firstName)
    );
  }

  get pageEmployees() {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    return this.filteredEmployees.slice(startIndex, startIndex + this.pageSize);
  }

  get totalPages() {
    return Math.ceil(this.filteredEmployees.length / this.pageSize);
  }

  goToFirstPage() {
    this.currentPage = 1;
  }

  goToPreviousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  goToNextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  goToLastPage() {
    this.currentPage = this.totalPages;
  }

  onLetterChange() {
    this.currentPage = 1;
  }

}
