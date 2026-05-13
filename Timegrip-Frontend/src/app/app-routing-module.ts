import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Employees } from './pages/employees/employees';
import { Shifts } from './pages/shifts/shifts';

const routes: Routes = [
  { path: 'employees', component: Employees },
  { path: 'shifts', component: Shifts },
  { path: '', redirectTo: 'employees', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
