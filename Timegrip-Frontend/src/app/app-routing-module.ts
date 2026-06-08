import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Employees } from './pages/employees/employees';
import { Shifts } from './pages/shifts/shifts';
import { CreateEmployee } from './pages/create-employee/create-employee';
import { EditEmployee } from './pages/edit-employee/edit-employee';
import { CreateShift } from './pages/create-shift/create-shift';
import { ShiftApprovals } from './pages/shift-approvals/shift-approvals';
import { Login } from './pages/login/login';

const routes: Routes = [
  { path: 'login', component: Login },

  { path: 'employees', component: Employees },
  { path: 'employees/create', component: CreateEmployee },
  { path: 'employees/edit/:id', component: EditEmployee },
  { path: 'shifts', component: Shifts },
  { path: 'shifts/create', component: CreateShift },
  { path: 'shifts/approvals', component: ShiftApprovals },

  { path: '', redirectTo: 'login', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
