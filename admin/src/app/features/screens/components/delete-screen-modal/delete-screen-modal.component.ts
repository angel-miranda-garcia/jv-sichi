import { Component, input, output } from '@angular/core';

import { ModalBaseComponent } from '../../../../shared/components/modal-base/modal-base.component';

@Component({
  selector: 'app-delete-screen-modal',
  standalone: true,
  imports: [ModalBaseComponent],
  templateUrl: './delete-screen-modal.component.html',
  styleUrl: './delete-screen-modal.component.scss',
})
export class DeleteScreenModalComponent {
  readonly isOpen = input(false);
  readonly screenName = input('');

  readonly closed = output<void>();
  readonly confirmed = output<void>();
}
