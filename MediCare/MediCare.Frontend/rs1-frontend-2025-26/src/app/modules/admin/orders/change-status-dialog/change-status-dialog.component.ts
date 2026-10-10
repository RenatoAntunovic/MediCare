import { Component, inject } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ListOrdersQueryDto } from '../../../../api-services/orders/orders-api.models';

export interface ChangeStatusDialogData {
  order: ListOrdersQueryDto;
}

/** Order status as shown in the dialog (id matches the OrderStatus table, name is a translation key). */
interface StatusOption {
  id: number;
  name: string;
}

/** Ids from the OrderStatus table (seed 1–5). */
const DRAFT = 1, CONFIRMED = 2, PAID = 3, COMPLETED = 4, CANCELLED = 5;

@Component({
  selector: 'app-change-status-dialog',
  standalone: false,
  templateUrl: './change-status-dialog.component.html',
  styleUrl: './change-status-dialog.component.scss'
})
export class ChangeStatusDialogComponent {
  private dialogRef = inject(MatDialogRef<ChangeStatusDialogComponent>);
  readonly data = inject<ChangeStatusDialogData>(MAT_DIALOG_DATA);

  /** Only the statuses the order can move to from its current status. */
  readonly availableStatuses: StatusOption[] = this.getNextStatuses(this.data.order.statusId);

  /** Reactive Forms: a new status must be chosen (the first allowed one is pre-selected). */
  readonly statusControl = new FormControl<number | null>(
    this.availableStatuses[0]?.id ?? null,
    Validators.required
  );

  readonly CANCELLED = CANCELLED;

  // === Status helpers ===

  getStatusLabel(status: StatusOption): string {
    return status.name;
  }

  /** Allowed transitions: Draft → Confirmed → Paid → Completed, cancel is possible until completed. */
  getNextStatuses(currentStatusId: number): StatusOption[] {
    switch (currentStatusId) {
      case DRAFT:
        return [this.option(CONFIRMED), this.option(CANCELLED)];
      case CONFIRMED:
        return [this.option(PAID), this.option(CANCELLED)];
      case PAID:
        return [this.option(COMPLETED), this.option(CANCELLED)];
      default:
        return []; // Completed and Cancelled have no next status
    }
  }

  getCurrentStatusLabel(): string {
    return `ORDERS.STATUS.${(this.data.order.statusName ?? 'UNKNOWN').toUpperCase()}`;
  }

  private option(id: number): StatusOption {
    const keys: Record<number, string> = {
      [DRAFT]: 'DRAFT',
      [CONFIRMED]: 'CONFIRMED',
      [PAID]: 'PAID',
      [COMPLETED]: 'COMPLETED',
      [CANCELLED]: 'CANCELLED'
    };
    return { id, name: `ORDERS.STATUS.${keys[id]}` };
  }

  // === Actions ===

  onConfirm(): void {
    if (!this.canConfirm()) return;
    this.dialogRef.close(this.statusControl.value);
  }

  onCancel(): void {
    this.dialogRef.close(undefined);
  }

  canConfirm(): boolean {
    return this.statusControl.valid && this.statusControl.value !== this.data.order.statusId;
  }
}