import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, output, signal } from '@angular/core';

import { Playlist } from '../../../../core/models/playlist.model';
import { PlaylistService } from '../../../../core/services/playlist.service';
import { ToastService } from '../../../../shared/components/toast/toast.service';
import { DeletePlaylistModalComponent } from '../delete-playlist-modal/delete-playlist-modal.component';

@Component({
  selector: 'app-playlist-list',
  standalone: true,
  imports: [CommonModule, DeletePlaylistModalComponent],
  templateUrl: './playlist-list.component.html',
  styleUrl: './playlist-list.component.scss',
})
export class PlaylistListComponent implements OnInit {
  private readonly playlistService = inject(PlaylistService);
  private readonly toastService = inject(ToastService);

  readonly createNew = output<void>();
  readonly edit = output<number>();

  readonly playlists = signal<Playlist[]>([]);
  readonly isLoading = signal(false);
  readonly showDeleteModal = signal(false);
  readonly selectedPlaylist = signal<Playlist | null>(null);

  ngOnInit(): void {
    this.loadPlaylists();
  }

  loadPlaylists(): void {
    this.isLoading.set(true);
    this.playlistService.getAll().subscribe({
      next: (list) => {
        this.playlists.set(list);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.show('Error al cargar playlists', 'error');
        this.isLoading.set(false);
      },
    });
  }

  openDeleteModal(playlist: Playlist): void {
    this.selectedPlaylist.set(playlist);
    this.showDeleteModal.set(true);
  }

  onDeleteConfirm(): void {
    const pl = this.selectedPlaylist();
    if (!pl) return;

    this.playlistService.delete(pl.id).subscribe({
      next: () => {
        this.toastService.show(`Playlist "${pl.name}" eliminada`);
        this.showDeleteModal.set(false);
        this.loadPlaylists();
      },
      error: () =>
        this.toastService.show('Error al eliminar playlist', 'error'),
    });
  }
}
