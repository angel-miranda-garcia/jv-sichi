import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';

import { PlaylistUsage } from '../../../../core/models/media-item.model';
import { ModalBaseComponent } from '../../../../shared/components/modal-base/modal-base.component';

@Component({
  selector: 'app-delete-media-modal',
  standalone: true,
  imports: [CommonModule, ModalBaseComponent],
  templateUrl: './delete-media-modal.component.html',
  styleUrl: './delete-media-modal.component.scss',
})
export class DeleteMediaModalComponent {
  readonly isOpen = input(false);
  readonly fileName = input('');
  readonly usage = input<PlaylistUsage[]>([]);

  readonly closed = output<void>();
  readonly confirmed = output<void>();
}
