import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import {
  FormControl,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  concatMap,
  from,
  map,
  of,
  switchMap,
  toArray,
} from 'rxjs';

import { MediaItem } from '../../../../core/models/media-item.model';
import {
  PlaylistDetail,
  PlaylistMediaItem,
} from '../../../../core/models/playlist.model';
import { MediaService } from '../../../../core/services/media.service';
import { PlaylistService } from '../../../../core/services/playlist.service';
import { ToastService } from '../../../../shared/components/toast/toast.service';
import { MediaSelectorModalComponent } from '../media-selector-modal/media-selector-modal.component';

@Component({
  selector: 'app-playlist-editor',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DragDropModule,
    MediaSelectorModalComponent,
  ],
  templateUrl: './playlist-editor.component.html',
  styleUrl: './playlist-editor.component.scss',
})
export class PlaylistEditorComponent implements OnInit {
  private readonly playlistService = inject(PlaylistService);
  private readonly mediaService = inject(MediaService);
  private readonly toastService = inject(ToastService);

  readonly playlistId = input<number | null>(null);
  readonly back = output<void>();

  readonly playlistDetail = signal<PlaylistDetail | null>(null);
  readonly nameControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required],
  });
  readonly items = signal<PlaylistMediaItem[]>([]);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly showMediaSelector = signal(false);
  readonly availableMedia = signal<MediaItem[]>([]);

  ngOnInit(): void {
    const id = this.playlistId();
    if (id != null) {
      this.loadPlaylist(id);
    } else {
      this.items.set([]);
      this.nameControl.setValue('');
    }
    this.loadAvailableMedia();
  }

  loadPlaylist(id: number): void {
    this.isLoading.set(true);
    this.playlistService.getById(id).subscribe({
      next: (detail) => {
        this.playlistDetail.set(detail);
        this.nameControl.setValue(detail.name);
        this.items.set([...detail.items]);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.show('Error al cargar playlist', 'error');
        this.isLoading.set(false);
      },
    });
  }

  loadAvailableMedia(): void {
    this.mediaService.getAll().subscribe({
      next: (list) => this.availableMedia.set(list),
    });
  }

  onDrop(event: CdkDragDrop<PlaylistMediaItem[]>): void {
    const currentItems = [...this.items()];
    moveItemInArray(currentItems, event.previousIndex, event.currentIndex);
    this.items.set(currentItems);
  }

  removeItem(index: number): void {
    const currentItems = [...this.items()];
    currentItems.splice(index, 1);
    this.items.set(currentItems);
  }

  updateDuration(index: number, event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    const currentItems = [...this.items()];
    const parsed = parseInt(value, 10);
    currentItems[index] = {
      ...currentItems[index],
      overrideDuration: Number.isNaN(parsed) ? null : parsed,
    };
    this.items.set(currentItems);
  }

  onMediaSelected(media: MediaItem): void {
    const newItem: PlaylistMediaItem = {
      mediaItemId: media.id,
      fileName: media.fileName,
      filePath: media.filePath,
      sortOrder: this.items().length,
      duration: media.durationSeconds,
      overrideDuration: null,
    };
    this.items.update((list) => [...list, newItem]);
    this.showMediaSelector.set(false);
  }

  save(): void {
    if (this.nameControl.invalid) {
      return;
    }

    this.isSaving.set(true);
    const existingId = this.playlistId();
    const saveOp$ =
      existingId != null
        ? this.playlistService.update(existingId, this.nameControl.value)
        : this.playlistService.create(this.nameControl.value);

    saveOp$
      .pipe(
        switchMap((saved) => {
          const id = saved.id;
          const currentItems = this.items();

          if (existingId == null) {
            if (currentItems.length === 0) {
              return of(id);
            }
            return from(currentItems).pipe(
              concatMap((item, idx) =>
                this.playlistService.addMedia(
                  id,
                  item.mediaItemId,
                  idx,
                  item.overrideDuration ?? undefined
                )
              ),
              toArray(),
              map(() => id)
            );
          }

          const reorderItems = currentItems.map((item, idx) => ({
            mediaItemId: item.mediaItemId,
            sortOrder: idx,
          }));
          return this.playlistService
            .reorder(id, reorderItems)
            .pipe(map(() => id));
        })
      )
      .subscribe({
        next: () => {
          this.toastService.show('Playlist guardada correctamente');
          this.isSaving.set(false);
          this.back.emit();
        },
        error: () => {
          this.toastService.show('Error al guardar playlist', 'error');
          this.isSaving.set(false);
        },
      });
  }
}
