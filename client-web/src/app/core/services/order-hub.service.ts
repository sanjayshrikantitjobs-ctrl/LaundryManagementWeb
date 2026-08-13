import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OrderStatus } from '../models/order.models';

export interface OrderStatusUpdate {
  orderId: string;
  orderNumber: string;
  newStatus: OrderStatus;
}

@Injectable({ providedIn: 'root' })
export class OrderHubService {
  private connection?: signalR.HubConnection;
  private readonly _statusUpdates = new Subject<OrderStatusUpdate>();
  readonly statusUpdates$ = this._statusUpdates.asObservable();

  async connect(): Promise<void> {
    if (this.connection) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/order-status`)
      .withAutomaticReconnect()
      .build();

    this.connection.on('OrderStatusChanged', (update: OrderStatusUpdate) => this._statusUpdates.next(update));

    await this.connection.start();
  }

  async joinOrderGroup(orderId: string): Promise<void> {
    await this.connection?.invoke('JoinOrderGroup', orderId);
  }

  async joinDashboardGroup(): Promise<void> {
    await this.connection?.invoke('JoinDashboardGroup');
  }

  async disconnect(): Promise<void> {
    await this.connection?.stop();
    this.connection = undefined;
  }
}
