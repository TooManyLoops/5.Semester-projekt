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

@NgModule({
  declarations: [App, Header, Sidebar, Employees, Shifts],
  imports: [BrowserModule, AppRoutingModule, FormsModule, HttpClientModule],
  providers: [provideBrowserGlobalErrorListeners()],
  bootstrap: [App],
})
export class AppModule {}
