import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-modal-base',
  standalone: true,
  templateUrl: './modal-base.component.html',
  styleUrl: './modal-base.component.scss',
})
export class ModalBaseComponent {
  readonly title = input.required<string>();
  readonly isOpen = input(false);
  readonly closed = output<void>();

  onOverlayClick(): void {
    this.closed.emit();
  }
}
