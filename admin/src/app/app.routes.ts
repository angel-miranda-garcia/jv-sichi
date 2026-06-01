import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { HistoryComponent } from './features/history/history.component';
import { LoginComponent } from './features/login/login.component';
import { MediaComponent } from './features/media/media.component';
import { PlaylistsComponent } from './features/playlists/playlists.component';
import { ScreensComponent } from './features/screens/screens.component';
import { SidebarComponent } from './layout/sidebar/sidebar.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: SidebarComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'screens', component: ScreensComponent },
      { path: 'playlists', component: PlaylistsComponent },
      { path: 'playlists/:id/edit', component: PlaylistsComponent },
      { path: 'media', component: MediaComponent },
      { path: 'history', component: HistoryComponent },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
