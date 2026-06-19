import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
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
    this.http.get<any[]>('http://localhost:5000/api/aggregate/employees')
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

  currentPage = 1;
  pageSize = 10;
  employeeSearch = '';
  showEmployeeSuggestions = false;


  get filteredEmployees() {
    let result = [...this.employees];
    const search = this.employeeSearch.trim().toLowerCase();

    if (search) {
      result = result.filter(emp =>
        this.getEmployeeFullName(emp).toLowerCase().startsWith(search)
      );
    }

    return result.sort((a, b) =>
      this.getEmployeeFullName(a).localeCompare(this.getEmployeeFullName(b))
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

  get employeeSuggestions() {
    const search = this.employeeSearch.trim().toLowerCase();

    if (!search) {
      return [];
    }

    return this.employees
      .filter(emp => this.getEmployeeFullName(emp).toLowerCase().startsWith(search))
      .sort((a, b) => this.getEmployeeFullName(a).localeCompare(this.getEmployeeFullName(b)))
      .slice(0, 8);
  }

  getEmployeeFullName(employee: any): string {
    return `${employee.firstName ?? ''} ${employee.lastName ?? ''}`.trim();
  }

  onEmployeeSearchChange() {
    this.currentPage = 1;
    this.showEmployeeSuggestions = this.employeeSearch.trim().length > 0;
  }

  selectEmployeeSuggestion(employee: any) {
    this.employeeSearch = this.getEmployeeFullName(employee);
    this.showEmployeeSuggestions = false;
    this.currentPage = 1;
  }

  hideEmployeeSuggestions() {
    setTimeout(() => {
      this.showEmployeeSuggestions = false;
      this.cdr.detectChanges();
    }, 150);
  }

}
