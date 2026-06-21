import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { HttpClientModule } from '@angular/common/http';

import { FormsModule } from '@angular/forms';
import { Header } from './header/header';
import { Sidebar } from './sidebar/sidebar';
import { Employees } from './pages/employees/employees';
import { Shifts } from './pages/shifts/shifts';
import { FullCalendarModule } from '@fullcalendar/angular';
import { CreateEmployee } from './pages/create-employee/create-employee';
import { EditEmployee } from './pages/edit-employee/edit-employee';
import { CreateShift } from './pages/create-shift/create-shift';
import { OpenShifts } from './pages/open-shifts/open-shifts';
import { ShiftApprovals } from './pages/shift-approvals/shift-approvals';

import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { Login } from './pages/login/login';

@NgModule({
  declarations: [
    App,
    Header,
    Sidebar,
    Employees,
    Shifts,
    CreateEmployee,
    EditEmployee,
    CreateShift,
    OpenShifts,
    ShiftApprovals,
    Login,
  ],

  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
    FullCalendarModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatInputModule,
  ],
  providers: [provideBrowserGlobalErrorListeners()],
  bootstrap: [App],
})
export class AppModule { }
