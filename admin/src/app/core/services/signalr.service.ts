import { Injectable, inject } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from '@microsoft/signalr';
import { Subject } from 'rxjs';

import { AuthService } from '../auth/auth.service';
import { Screen } from '../models/screen.model';

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private readonly authService = inject(AuthService);

  private connection: HubConnection | null = null;
  private readonly screenUpdated = new Subject<Screen>();
  readonly screenUpdated$ = this.screenUpdated.asObservable();

  startConnection(): void {
    if (this.connection?.state === HubConnectionState.Connected) {
      return;
    }

    this.connection = new HubConnectionBuilder()
      .withUrl('/signalr/player')
      .withAutomaticReconnect()
      .build();

    this.connection.on('screenStatusUpdated', (screen: Screen) => {
      this.screenUpdated.next(screen);
    });

    void this.connection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch((err) => console.error('SignalR error:', err));
  }

  stopConnection(): void {
    void this.connection?.stop();
    this.connection = null;
  }
}
