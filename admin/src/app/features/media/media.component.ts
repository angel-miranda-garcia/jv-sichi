import { CommonModule } from '@angular/common';
import { HttpErrorResponse, HttpEventType } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';

import { MediaItem, PlaylistUsage } from '../../core/models/media-item.model';
import { MediaService } from '../../core/services/media.service';
import { ToastService } from '../../shared/components/toast/toast.service';
import { DeleteMediaModalComponent } from './components/delete-media-modal/delete-media-modal.component';
import { MediaUsageModalComponent } from './components/media-usage-modal/media-usage-modal.component';

@Component({
  selector: 'app-media',
  standalone: true,
  imports: [
    CommonModule,
    MediaUsageModalComponent,
    DeleteMediaModalComponent,
  ],
  templateUrl: './media.component.html',
  styleUrl: './media.component.scss',
})
export class MediaComponent implements OnInit {
  private readonly mediaService = inject(MediaService);
  private readonly toastService = inject(ToastService);

  readonly mediaItems = signal<MediaItem[]>([]);
  readonly isLoading = signal(false);
  readonly uploadProgress = signal<number | null>(null);
  readonly uploadFileName = signal<string | null>(null);

  readonly showUsageModal = signal(false);
  readonly showDeleteModal = signal(false);
  readonly selectedItem = signal<MediaItem | null>(null);
  readonly selectedItemUsage = signal<PlaylistUsage[]>([]);
  readonly isLoadingUsage = signal(false);

  ngOnInit(): void {
    this.loadMedia();
  }

  loadMedia(): void {
    this.isLoading.set(true);
    this.mediaService.getAll().subscribe({
      next: (list) => {
        this.mediaItems.set(list);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.show('Error al cargar archivos', 'error');
        this.isLoading.set(false);
      },
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    this.uploadFile(input.files[0]);
    input.value = '';
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    const file = event.dataTransfer?.files[0];
    if (file) {
      this.uploadFile(file);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  uploadFile(file: File): void {
    this.uploadFileName.set(file.name);
    this.uploadProgress.set(0);

    this.mediaService.upload(file).subscribe({
      next: (event) => {
        if (event.type === HttpEventType.UploadProgress) {
          const progress = event.total
            ? Math.round((100 * event.loaded) / event.total)
            : 0;
          this.uploadProgress.set(progress);
        }
        if (event.type === HttpEventType.Response) {
          this.toastService.show('Archivo subido correctamente');
          this.uploadProgress.set(null);
          this.uploadFileName.set(null);
          this.loadMedia();
        }
      },
      error: (err: HttpErrorResponse) => {
        const msg =
          err.status === 400
            ? ((err.error as { message?: string })?.message ?? 'Archivo inválido')
            : err.status === 429
              ? 'Demasiadas subidas. Intenta en un minuto.'
              : 'Error al subir archivo';
        this.toastService.show(msg, 'error');
        this.uploadProgress.set(null);
        this.uploadFileName.set(null);
      },
    });
  }

  openUsageModal(item: MediaItem): void {
    this.selectedItem.set(item);
    this.showUsageModal.set(true);
    this.isLoadingUsage.set(true);
    this.selectedItemUsage.set([]);

    this.mediaService.getUsage(item.id).subscribe({
      next: (usage) => {
        this.selectedItemUsage.set(usage);
        this.isLoadingUsage.set(false);
      },
      error: () => {
        this.toastService.show('Error al cargar uso', 'error');
        this.isLoadingUsage.set(false);
      },
    });
  }

  openDeleteModal(item: MediaItem): void {
    this.selectedItem.set(item);
    this.showUsageModal.set(false);
    this.mediaService.getUsage(item.id).subscribe({
      next: (usage) => this.selectedItemUsage.set(usage),
    });
    this.showDeleteModal.set(true);
  }

  onDeleteConfirm(): void {
    const item = this.selectedItem();
    if (!item) return;

    this.mediaService.delete(item.id).subscribe({
      next: () => {
        this.toastService.show('Archivo eliminado correctamente');
        this.showDeleteModal.set(false);
        this.loadMedia();
      },
      error: () =>
        this.toastService.show('Error al eliminar archivo', 'error'),
    });
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1048576) {
      return `${(bytes / 1024).toFixed(1)} KB`;
    }
    return `${(bytes / 1048576).toFixed(1)} MB`;
  }

  formatDuration(seconds: number): string {
    if (seconds === 0) {
      return '—';
    }
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, '0')}`;
  }
}
