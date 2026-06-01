import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/** Layout shell — implementación completa en 4.6-b */
@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {}
