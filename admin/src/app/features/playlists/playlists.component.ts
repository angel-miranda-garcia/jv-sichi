import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { PlaylistEditorComponent } from './components/playlist-editor/playlist-editor.component';
import { PlaylistListComponent } from './components/playlist-list/playlist-list.component';

@Component({
  selector: 'app-playlists',
  standalone: true,
  imports: [PlaylistListComponent, PlaylistEditorComponent],
  templateUrl: './playlists.component.html',
})
export class PlaylistsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly view = signal<'list' | 'editor'>('list');
  readonly editingPlaylistId = signal<number | null>(null);

  ngOnInit(): void {
    this.syncFromRoute();
    this.route.paramMap.subscribe(() => this.syncFromRoute());
  }

  private syncFromRoute(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const isEditRoute = this.route.snapshot.url.some((s) => s.path === 'edit');

    if (idParam && isEditRoute) {
      this.editingPlaylistId.set(Number(idParam));
      this.view.set('editor');
      return;
    }

    if (!idParam) {
      if (this.view() === 'editor' && this.editingPlaylistId() === null) {
        return;
      }
      this.view.set('list');
      this.editingPlaylistId.set(null);
    }
  }

  onCreateNew(): void {
    this.editingPlaylistId.set(null);
    this.view.set('editor');
  }

  onEdit(playlistId: number): void {
    this.editingPlaylistId.set(playlistId);
    this.view.set('editor');
    void this.router.navigate(['/playlists', playlistId, 'edit']);
  }

  onBack(): void {
    this.view.set('list');
    this.editingPlaylistId.set(null);
    void this.router.navigate(['/playlists']);
  }
}
