import { Component, input, output } from '@angular/core';

import { ModalBaseComponent } from '../../../../shared/components/modal-base/modal-base.component';

@Component({
  selector: 'app-delete-playlist-modal',
  standalone: true,
  imports: [ModalBaseComponent],
  templateUrl: './delete-playlist-modal.component.html',
  styleUrl: './delete-playlist-modal.component.scss',
})
export class DeletePlaylistModalComponent {
  readonly isOpen = input(false);
  readonly playlistName = input('');

  readonly closed = output<void>();
  readonly confirmed = output<void>();
}
