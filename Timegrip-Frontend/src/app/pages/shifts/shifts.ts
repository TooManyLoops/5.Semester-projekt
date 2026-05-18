import { Component } from '@angular/core';
import { CalendarOptions } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import daLocale from '@fullcalendar/core/locales/da';

@Component({
  selector: 'app-shifts',
  standalone: false,
  templateUrl: './shifts.html',
  styleUrl: './shifts.css'
})
export class Shifts {

  calendarOptions: CalendarOptions = {
    plugins: [
      dayGridPlugin,
      timeGridPlugin,
      interactionPlugin
    ],

    locale: daLocale,

    initialView: 'timeGridWeek',

    selectable: true,

    events: [
      {
        title: 'Khanh - 08:00-16:00',
        start: '2026-05-18T08:00:00',
        end: '2026-05-18T16:00:00'
      }
    ],

    dateClick: (info) => {
      console.log('Klikket dato:', info.dateStr);
      alert('Opret vagt på: ' + info.dateStr);
    }
  };
}
