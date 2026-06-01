import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';

import { PendingScreen, Screen } from '../../core/models/screen.model';
import { Playlist } from '../../core/models/playlist.model';
import { ScreenService } from '../../core/services/screen.service';
import { PlaylistService } from '../../core/services/playlist.service';
import { ToastService } from '../../shared/components/toast/toast.service';
import { AssignPlaylistModalComponent } from './components/assign-playlist-modal/assign-playlist-modal.component';
import { DeleteScreenModalComponent } from './components/delete-screen-modal/delete-screen-modal.component';

@Component({
  selector: 'app-screens',
  standalone: true,
  imports: [
    CommonModule,
    AssignPlaylistModalComponent,
    DeleteScreenModalComponent,
  ],
  templateUrl: './screens.component.html',
  styleUrl: './screens.component.scss',
})
export class ScreensComponent implements OnInit {
  private readonly screenService = inject(ScreenService);
  private readonly playlistService = inject(PlaylistService);
  private readonly toastService = inject(ToastService);

  readonly activeTab = signal<'active' | 'pending'>('active');
  readonly screens = signal<Screen[]>([]);
  readonly pendingScreens = signal<PendingScreen[]>([]);
  readonly playlists = signal<Playlist[]>([]);
  readonly isLoading = signal(false);

  readonly showAssignModal = signal(false);
  readonly showDeleteModal = signal(false);
  readonly selectedScreen = signal<Screen | null>(null);

  ngOnInit(): void {
    this.loadScreens();
    this.loadPlaylists();
  }

  loadScreens(): void {
    this.isLoading.set(true);
    forkJoin({
      active: this.screenService.getScreens(),
      pending: this.screenService.getPending(),
    }).subscribe({
      next: ({ active, pending }) => {
        this.screens.set(active);
        this.pendingScreens.set(pending);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.show('Error al cargar pantallas', 'error');
        this.isLoading.set(false);
      },
    });
  }

  loadPlaylists(): void {
    this.playlistService.getAll().subscribe({
      next: (list) => this.playlists.set(list),
    });
  }

  setTab(tab: 'active' | 'pending'): void {
    this.activeTab.set(tab);
  }

  openAssignModal(screen: Screen): void {
    this.selectedScreen.set(screen);
    this.showAssignModal.set(true);
  }

  onAssignConfirm(playlistId: number | null): void {
    const screen = this.selectedScreen();
    if (!screen) return;

    this.screenService.assignPlaylist(screen.id, playlistId).subscribe({
      next: () => {
        this.toastService.show('Playlist asignada correctamente');
        this.showAssignModal.set(false);
        this.loadScreens();
      },
      error: () =>
        this.toastService.show('Error al asignar playlist', 'error'),
    });
  }

  forceRefresh(screen: Screen): void {
    this.screenService.forceRefresh(screen.id).subscribe({
      next: () =>
        this.toastService.show(`Refresh enviado a ${screen.name}`),
      error: () =>
        this.toastService.show('Error al enviar refresh', 'error'),
    });
  }

  openDeleteModal(screen: Screen): void {
    this.selectedScreen.set(screen);
    this.showDeleteModal.set(true);
  }

  onDeleteConfirm(): void {
    const screen = this.selectedScreen();
    if (!screen) return;

    this.screenService.delete(screen.id).subscribe({
      next: () => {
        this.toastService.show(`Pantalla ${screen.name} eliminada`);
        this.showDeleteModal.set(false);
        this.loadScreens();
      },
      error: (err: HttpErrorResponse) => {
        const msg =
          err.status === 409
            ? 'No se puede eliminar: tiene playlist asignada'
            : 'Error al eliminar pantalla';
        this.toastService.show(msg, 'error');
      },
    });
  }

  approveScreen(screen: PendingScreen): void {
    this.screenService.approve(screen.id).subscribe({
      next: () => {
        this.toastService.show(`Pantalla ${screen.deviceId} aprobada`);
        this.loadScreens();
      },
      error: () =>
        this.toastService.show('Error al aprobar pantalla', 'error'),
    });
  }

  rejectScreen(screen: PendingScreen): void {
    this.screenService.reject(screen.id).subscribe({
      next: () => {
        this.toastService.show(`Dispositivo ${screen.deviceId} rechazado`);
        this.loadScreens();
      },
      error: () =>
        this.toastService.show('Error al rechazar dispositivo', 'error'),
    });
  }

  formatLastPing(lastPing: string | null): string {
    if (!lastPing) {
      return 'Nunca';
    }

    const diffMs = Date.now() - new Date(lastPing).getTime();
    const secs = Math.floor(diffMs / 1000);

    if (secs < 60) {
      return `Hace ${secs} seg`;
    }
    if (secs < 3600) {
      return `Hace ${Math.floor(secs / 60)} min`;
    }
    return `Hace ${Math.floor(secs / 3600)} horas`;
  }
}
