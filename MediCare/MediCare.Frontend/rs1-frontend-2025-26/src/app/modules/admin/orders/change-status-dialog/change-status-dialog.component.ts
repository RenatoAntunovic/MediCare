import { Component, inject, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import {ListOrdersQueryDto} from '../../../../api-services/orders/orders-api.models';
import {OrderStatusHelper} from '../../../../api-services/orders/order-status.helper';

export interface ChangeStatusDialogData {
  order: ListOrdersQueryDto;
}

@Component({
  selector: 'app-change-status-dialog',
  standalone: false,
  templateUrl: './change-status-dialog.component.html',
  styleUrl: './change-status-dialog.component.scss'
})
export class ChangeStatusDialogComponent {
  private dialogRef = inject(MatDialogRef<ChangeStatusDialogComponent>);

  selectedStatusId?: number;
  availableStatuses: { id: number; name: string }[] = [
  { id: 1, name: 'Draft' },
  { id: 2, name: 'Confirmed' },
  { id: 3, name: 'Paid' },
  { id: 4, name: 'Completed' },
  { id: 6, name: 'Cancelled' },
];

  constructor(@Inject(MAT_DIALOG_DATA) public data: ChangeStatusDialogData) {
    this.selectedStatusId = data.order.statusId;

    // Pre-select first available status
   if (!this.selectedStatusId && this.availableStatuses.length > 0) {
    this.selectedStatusId = this.availableStatuses[0].id;
    }

  }

  // === Status Helpers ===

getStatusLabel(status: { id: number; name: string }): string {
  return status.name;
}

getNextStatuses(currentStatusId: number): { id: number; name: string }[] {
  switch (currentStatusId) {
    case 1: // Draft
      return [
        { id: 2, name: 'Confirmed' },
        { id: 6, name: 'Cancelled' }
      ];
    case 2: // Confirmed
      return [
        { id: 3, name: 'Paid' },
        { id: 6, name: 'Cancelled' }
      ];
    case 3: // Paid
      return [
        { id: 4, name: 'Completed' },
        { id: 6, name: 'Cancelled' }
      ];
    default:
      return []; // Completed i Cancelled nemaju dalje
  }
}


getStatusIcon(statusId: number): string {
  switch(statusId) {
    case 1: return 'draft_icon';
    case 2: return 'check_circle';
    case 3: return 'payment';
    case 4: return 'done_all';
    case 6: return 'cancel';
    default: return 'help';
  }
}

getStatusClass(statusId: number): string {
  switch(statusId) {
    case 1: return 'status-draft';
    case 2: return 'status-confirmed';
    case 3: return 'status-paid';
    case 4: return 'status-completed';
    case 6: return 'status-cancelled';
    default: return '';
  }
}

getCurrentStatusLabel(): string {
  return this.data.order.statusName;
}

getCurrentStatusClass(): string {
  return this.getStatusClass(this.data.order.statusId);
}

getCurrentStatusIcon(): string {
  return this.getStatusIcon(this.data.order.statusId);
}


  // === Actions ===

  onConfirm(): void {
    if (this.selectedStatusId !== undefined &&
        this.selectedStatusId !== this.data.order.statusId) {

      this.dialogRef.close(this.selectedStatusId);
    }
  }

  onCancel(): void {
    this.dialogRef.close(undefined);
  }

  canConfirm(): boolean {
    return this.selectedStatusId !== undefined &&
           this.selectedStatusId !== this.data.order.statusId;
  }

}
