import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';

import { PlaylistUsage } from '../../../../core/models/media-item.model';
import { ModalBaseComponent } from '../../../../shared/components/modal-base/modal-base.component';

@Component({
  selector: 'app-media-usage-modal',
  standalone: true,
  imports: [CommonModule, ModalBaseComponent],
  templateUrl: './media-usage-modal.component.html',
  styleUrl: './media-usage-modal.component.scss',
})
export class MediaUsageModalComponent {
  readonly isOpen = input(false);
  readonly fileName = input('');
  readonly usage = input<PlaylistUsage[]>([]);
  readonly isLoading = input(false);

  readonly closed = output<void>();
}
