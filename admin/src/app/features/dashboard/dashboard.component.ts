import { CommonModule } from '@angular/common';
import {
  Component,
  DestroyRef,
  OnDestroy,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { Screen } from '../../core/models/screen.model';
import { ScreenService } from '../../core/services/screen.service';
import { SignalrService } from '../../core/services/signalr.service';
import { ToastService } from '../../shared/components/toast/toast.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly screenService = inject(ScreenService);
  private readonly signalrService = inject(SignalrService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly screens = signal<Screen[]>([]);
  readonly isLoading = signal(false);

  readonly totalScreens = computed(() => this.screens().length);
  readonly onlineScreens = computed(
    () => this.screens().filter((s) => s.isOnline).length
  );
  readonly offlineScreens = computed(
    () => this.screens().filter((s) => !s.isOnline).length
  );

  ngOnInit(): void {
    this.loadScreens();
    this.signalrService.startConnection();

    this.signalrService.screenUpdated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((updatedScreen) => {
        this.screens.update((list) =>
          list.map((s) => (s.id === updatedScreen.id ? updatedScreen : s))
        );
      });
  }

  ngOnDestroy(): void {
    this.signalrService.stopConnection();
  }

  loadScreens(): void {
    this.isLoading.set(true);
    this.screenService.getScreens().subscribe({
      next: (list) => {
        this.screens.set(list);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.show('Error al cargar pantallas', 'error');
        this.isLoading.set(false);
      },
    });
  }

  forceRefresh(screen: Screen): void {
    this.screenService.forceRefresh(screen.id).subscribe({
      next: () =>
        this.toastService.show(`Refresh enviado a ${screen.name}`),
      error: () =>
        this.toastService.show('Error al enviar refresh', 'error'),
    });
  }

  formatLastPing(lastPing: string | null): string {
    if (!lastPing) {
      return 'Nunca';
    }

    const diffMs = Date.now() - new Date(lastPing).getTime();
    const secs = Math.floor(diffMs / 1000);

    if (secs < 60) {
      return `Hace ${secs} seg`;
    }
    if (secs < 3600) {
      return `Hace ${Math.floor(secs / 60)} min`;
    }
    return `Hace ${Math.floor(secs / 3600)} horas`;
  }
}
