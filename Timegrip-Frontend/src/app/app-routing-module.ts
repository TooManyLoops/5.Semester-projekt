import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Employees } from './pages/employees/employees';
import { Shifts } from './pages/shifts/shifts';
import { CreateEmployee } from './pages/create-employee/create-employee';
import { EditEmployee } from './pages/edit-employee/edit-employee';

const routes: Routes = [
  { path: 'employees', component: Employees },
  { path: 'employees/create', component: CreateEmployee },
  { path: 'employees/edit/:id', component: EditEmployee },
  { path: 'shifts', component: Shifts },

  { path: '', redirectTo: 'employees', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
