import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../core/auth/auth.service';
import { ScreenService } from '../../core/services/screen.service';
import { ToastComponent } from '../../shared/components/toast/toast.component';

interface NavItem {
  path: string;
  icon: string;
  label: string;
  badge?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ToastComponent,
  ],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent implements OnInit {
  readonly authService = inject(AuthService);
  private readonly screenService = inject(ScreenService);
  readonly isCollapsed = signal(false);
  readonly pendingCount = signal(0);

  readonly navItems: NavItem[] = [
    { path: '/dashboard', icon: '⊞', label: 'Dashboard' },
    { path: '/screens', icon: '🖥', label: 'Pantallas', badge: true },
    { path: '/playlists', icon: '▶', label: 'Playlists' },
    { path: '/media', icon: '🎬', label: 'Media' },
    { path: '/history', icon: '📋', label: 'Historial' },
  ];

  ngOnInit(): void {
    this.loadPendingCount();
  }

  loadPendingCount(): void {
    this.screenService.getPending().subscribe({
      next: (list) => this.pendingCount.set(list.length),
    });
  }

  toggleSidebar(): void {
    this.isCollapsed.update((v) => !v);
  }

  logout(): void {
    this.authService.logout();
  }
}
