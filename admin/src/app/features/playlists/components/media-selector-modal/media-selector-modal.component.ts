import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';

import { MediaItem } from '../../../../core/models/media-item.model';
import { ModalBaseComponent } from '../../../../shared/components/modal-base/modal-base.component';

@Component({
  selector: 'app-media-selector-modal',
  standalone: true,
  imports: [CommonModule, ModalBaseComponent],
  templateUrl: './media-selector-modal.component.html',
  styleUrl: './media-selector-modal.component.scss',
})
export class MediaSelectorModalComponent {
  readonly isOpen = input(false);
  readonly mediaItems = input<MediaItem[]>([]);

  readonly closed = output<void>();
  readonly selected = output<MediaItem>();
}
