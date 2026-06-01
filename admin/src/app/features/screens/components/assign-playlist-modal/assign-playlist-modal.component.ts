import {
  Component,
  OnChanges,
  SimpleChanges,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';

import { Playlist } from '../../../../core/models/playlist.model';
import { ModalBaseComponent } from '../../../../shared/components/modal-base/modal-base.component';

@Component({
  selector: 'app-assign-playlist-modal',
  standalone: true,
  imports: [ModalBaseComponent, FormsModule],
  templateUrl: './assign-playlist-modal.component.html',
  styleUrl: './assign-playlist-modal.component.scss',
})
export class AssignPlaylistModalComponent implements OnChanges {
  readonly isOpen = input(false);
  readonly playlists = input<Playlist[]>([]);
  readonly currentPlaylistId = input<number | null>(null);

  readonly closed = output<void>();
  readonly confirmed = output<number | null>();

  readonly selectedPlaylistId = signal<number | null>(null);
  selectedPlaylistIdValue: number | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen']?.currentValue === true) {
      const current = this.currentPlaylistId();
      this.selectedPlaylistId.set(current);
      this.selectedPlaylistIdValue = current;
    }
  }

  onConfirm(): void {
    this.confirmed.emit(this.selectedPlaylistIdValue);
  }
}
