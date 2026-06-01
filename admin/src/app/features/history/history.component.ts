import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';

import { AuditLog } from '../../core/models/audit-log.model';
import { AuditService } from '../../core/services/audit.service';
import { ToastService } from '../../shared/components/toast/toast.service';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './history.component.html',
  styleUrl: './history.component.scss',
})
export class HistoryComponent implements OnInit {
  private readonly auditService = inject(AuditService);
  private readonly toastService = inject(ToastService);

  readonly auditLogs = signal<AuditLog[]>([]);
  readonly total = signal(0);
  readonly currentPage = signal(1);
  readonly pageSize = 10;
  readonly isLoading = signal(false);
  readonly filterAction = signal('all');

  readonly totalPages = computed(() =>
    Math.ceil(this.total() / this.pageSize)
  );

  readonly pageNumbers = computed(() => {
    const pages: number[] = [];
    for (let i = 1; i <= this.totalPages(); i++) {
      pages.push(i);
    }
    return pages;
  });

  readonly rangeStart = computed(() =>
    this.total() === 0 ? 0 : (this.currentPage() - 1) * this.pageSize + 1
  );

  readonly rangeEnd = computed(() =>
    Math.min(this.currentPage() * this.pageSize, this.total())
  );

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading.set(true);
    this.auditService.getAll(this.currentPage(), this.pageSize).subscribe({
      next: (result) => {
        this.auditLogs.set(result.items);
        this.total.set(result.total);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.show('Error al cargar historial', 'error');
        this.isLoading.set(false);
      },
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }
    this.currentPage.set(page);
    this.loadLogs();
  }

  onFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterAction.set(value);
  }

  getActionColor(action: string): string {
    if (action.includes('Created') || action.includes('Approved')) {
      return 'var(--status-online)';
    }
    if (action.includes('Deleted') || action.includes('Rejected')) {
      return 'var(--status-offline)';
    }
    if (action.includes('Updated') || action.includes('Assigned')) {
      return '#1976D2';
    }
    return 'var(--text-muted)';
  }
}
