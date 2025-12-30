import {Component, inject, Inject} from '@angular/core';
import {GetOrderByIdQueryDto} from '../../../../api-services/orders/orders-api.models';
import { OrderStatusHelper } from '../../../../api-services/orders/order-status.helper';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {OrdersApiService} from '../../../../api-services/orders/orders-api.service';

export interface OrderDetailsDialogData {
  orderId: number;
}

@Component({
  selector: 'app-admin-orders-details-dialog',
  standalone: false,
  templateUrl: './order-details-dialog.component.html',
  styleUrl: './order-details-dialog.component.scss',
})
export class OrderDetailsDialogComponent {
  private ordersApi = inject(OrdersApiService);
  private dialogRef = inject(MatDialogRef<OrderDetailsDialogComponent>);

  order?: GetOrderByIdQueryDto;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(@Inject(MAT_DIALOG_DATA) public data: OrderDetailsDialogData) {}

  ngOnInit(): void {
  setTimeout(() => {
    this.loadOrderDetails();
  });
  }

  private loadOrderDetails(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.ordersApi.getById(this.data.orderId).subscribe({
      next: (order) => {
        this.order = order;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load order details';
        this.isLoading = false;
        console.error('Load order details error:', err);
      }
    });
  }

  getTotalAmount(): number {
  if (!this.order?.items) return 0;
  return this.order.items.reduce((sum, item) => sum + item.price * item.quantity, 0);
}
  // === Status Helpers ===

getStatusLabel(status: { id: number; name: string } | number): string {
  // Ako je broj, možeš vratiti odgovarajući label ili koristiti statusName
  if (typeof status === 'number') {
    switch (status) {
      case 1: return 'ORDERS.STATUS.DRAFT';
      case 2: return 'ORDERS.STATUS.CONFIRMED';
      case 3: return 'ORDERS.STATUS.PAID';
      case 4: return 'ORDERS.STATUS.COMPLETED';
      case 5: return 'ORDERS.STATUS.CANCELLED';
      default: return 'ORDERS.STATUS.UNKNOWN';
    }
  } else {
    // status je objekat {id, name}
    return status.name;
  }
}

getStatusIcon(statusId: number): string {
  switch(statusId) {
    case 1: return 'edit_note';
    case 2: return 'check_circle';
    case 3: return 'payment';
    case 4: return 'done_all';
    case 6: return 'cancel';
    default: return 'help_outline';
  }
}

getStatusClass(statusId: number): string {
  switch(statusId) {
    case 1: return 'status-draft';
    case 2: return 'status-confirmed';
    case 3: return 'status-paid';
    case 4: return 'status-completed';
    case 6: return 'status-cancelled';
    default: return 'status-unknown';
  }
}

  // === Display Helpers ===

  getCustomerName(): string {
    if (!this.order) return '';
    return `${this.order.user.userFirstname} ${this.order.user.userLastname}`;
  }

  getCustomerAddress(): string {
    if (!this.order) return '';
    return `${this.order.user.userAddress}, ${this.order.user.userCity}`;
  }

  // === Actions ===

  onClose(): void {
    this.dialogRef.close(false);
  }

  retry(): void {
    this.loadOrderDetails();
  }
}
