import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-modal-base',
  standalone: true,
  templateUrl: './modal-base.component.html',
  styleUrl: './modal-base.component.scss',
})
export class ModalBaseComponent {
  readonly title = input.required<string>();
  readonly visible = input(false, { alias: 'isOpen' });
  readonly closed = output<void>();

  onBackdropClick(): void {
    this.closed.emit();
  }

  onCloseClick(): void {
    this.closed.emit();
  }
}
