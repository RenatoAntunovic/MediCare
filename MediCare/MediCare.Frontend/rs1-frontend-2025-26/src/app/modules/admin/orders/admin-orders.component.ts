import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {Subject} from 'rxjs';
import {debounceTime, distinctUntilChanged, takeUntil} from 'rxjs/operators';
import {FormControl} from '@angular/forms';
import {BaseListPagedComponent} from '../../../core/components/base-classes/base-list-paged-component';
import {ListOrdersQueryDto, ListOrdersRequest} from '../../../api-services/orders/orders-api.models';
import {OrdersApiService} from '../../../api-services/orders/orders-api.service';
import {ToasterService} from '../../../core/services/toaster.service';
import {OrderStatusHelper} from '../../../api-services/orders/order-status.helper';
import {ChangeStatusDialogComponent} from './change-status-dialog/change-status-dialog.component';
import {OrderDetailsDialogComponent} from './admin-orders-details-dialog/order-details-dialog.component';

@Component({
  selector: 'app-admin-orders',
  standalone: false,
  templateUrl: './admin-orders.component.html',
  styleUrl: './admin-orders.component.scss',
})
export class AdminOrdersComponent
  extends BaseListPagedComponent<ListOrdersQueryDto, ListOrdersRequest>
  implements OnInit, OnDestroy
{
  private ordersApi = inject(OrdersApiService);
  private dialog = inject(MatDialog);
  private toaster = inject(ToasterService);
  private destroy$ = new Subject<void>();

  // Table columns
  displayedColumns: string[] = [
    'id',
    'customer',
    'orderDate',
    'totalAmount',
    'status',
    'actions'
  ];

  // Search control with debounce
  searchControl = new FormControl('');

  // Status filter
statusFilter: number | null = null;
statusOptions: { id: number; name: string; icon:string }[] = [
  { id: 1, name: 'ORDERS.STATUS.DRAFT', icon: 'edit_note' },
  { id: 2, name: 'ORDERS.STATUS.CONFIRMED', icon: 'check_circle' },
  { id: 3, name: 'ORDERS.STATUS.PAID', icon: 'payment' },
  { id: 4, name: 'ORDERS.STATUS.COMPLETED', icon: 'done_all' },
  { id: 5, name: 'ORDERS.STATUS.CANCELLED', icon: 'cancel' },
];


  constructor() {
    super();
    this.request = new ListOrdersRequest();
    this.request.paging.pageSize = 20;
  }

  ngOnInit(): void {
    this.initList();
    this.setupSearchDebounce();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Setup search with debounce and minimum length
   */
  private setupSearchDebounce(): void {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(400), // Wait 400ms after user stops typing
        distinctUntilChanged(), // Only if value actually changed
        takeUntil(this.destroy$)
      )
      .subscribe((searchTerm) => {
        // Only search if 3+ characters or empty (to clear)
        if (!searchTerm || searchTerm.length >= 3) {
          this.onSearchChange(searchTerm || '');
        }
      });
  }

  private lastRequestTime = 0;
private requestCooldown = 2000; // 2 sekunde cooldown

private canMakeRequest(): boolean {
  const now = Date.now();
  if (now - this.lastRequestTime < this.requestCooldown) {
    return false; // još nije prošlo 2 sekunde
  }
  this.lastRequestTime = now;
  return true;
}

  protected loadPagedData(): void {
      if (!this.canMakeRequest()) {
    this.toaster.error('Too many requests, please wait a few seconds.');
    return;
  }

    this.startLoading();

    this.ordersApi.list(this.request).subscribe({
      next: (response) => {
        this.handlePageResult(response);
        this.stopLoading();
      },
      error: (err) => {
        this.stopLoading('Failed to load orders');
        console.error('Load orders error:', err);
      },
    });
  }

  // === Filters ===

  onSearchChange(searchTerm: string): void {
    this.request.search = searchTerm;
    this.request.paging.page = 1; // Reset to first page
    this.loadPagedData();
  }

onStatusFilterChange(status: number | null): void {
  this.statusFilter = status;
  this.request.paging.page = 1;
  this.loadPagedData();
}

  clearFilters(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.statusFilter = null;
    this.request.search = null;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  // === Actions ===

  onViewDetails(order: ListOrdersQueryDto, event?: MouseEvent): void {
    // spriječi da klik sa dugmeta ode na <tr> i ponovo otvori dialog
    event?.stopPropagation();

    console.log('Order ID being sent to dialog:', order.id);

    const dialogRef = this.dialog.open(OrderDetailsDialogComponent, {
      width: '900px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      data: {
        orderId: order.id
      },
      panelClass: 'order-details-dialog'
    });

    dialogRef.afterClosed().subscribe((changed: boolean) => {
      if (changed) {
        this.loadPagedData(); // Reload if status changed
      }
    });
  }

 onChangeStatus(order: ListOrdersQueryDto, event?: Event): void {
  event?.stopPropagation();

  const dialogRef = this.dialog.open(ChangeStatusDialogComponent, {
    width: '500px',
    maxWidth: '90vw',
    data: { order },
    panelClass: 'change-status-dialog'
  });

  dialogRef.afterClosed().subscribe((newStatusId: number | undefined) => {
    if (newStatusId !== undefined) {
      this.changeOrderStatus(order.id, newStatusId);
    }
  });
}

private changeOrderStatus(orderId: number, newStatusId: number): void {
  console.log('Changing order status', { orderId, newStatusId });
  this.startLoading();

  this.ordersApi.changeStatus(orderId, newStatusId).subscribe({
    next: () => {
      this.toaster.success('Order status updated successfully');
      this.loadPagedData();
    },
    error: (err) => {
      this.stopLoading();
      const errorMessage = this.extractErrorMessage(err);
      this.toaster.error(errorMessage || 'Failed to update order status');
    }
  });
}


  // === Status Helpers (for template) ===

getStatusLabel(statusId: number): string {
  switch(statusId) {
    case 1: return 'ORDERS.STATUS.DRAFT';
    case 2: return 'ORDERS.STATUS.CONFIRMED';
    case 3: return 'ORDERS.STATUS.PAID';
    case 4: return 'ORDERS.STATUS.COMPLETED';
    case 6: return 'ORDERS.STATUS.CANCELLED';
    default: return 'ORDERS.STATUS.UNKNOWN';
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

  canChangeStatus(order: ListOrdersQueryDto): boolean {
    // Can change if not in final state
    return order.statusId !== 4 &&
      order.statusId !== 5;
  }

  // === Display Helpers ===

  getCustomerName(order: ListOrdersQueryDto): string {
    return `${order.user.userFirstname} ${order.user.userLastname}`;
  }

  getCustomerAddress(order: ListOrdersQueryDto): string {
    if (!order.user) return '';
    const address = order.user.userAddress ?? '';
    const city = order.user.userCity ?? '';
    return address && city ? `${address}, ${city}` : address || city;
  }


  /**
   * Extract user-friendly error message from HTTP error response
   */
  private extractErrorMessage(err: any): string | null {
    if (err?.error) {
      if (typeof err.error === 'string') {
        return err.error;
      }

      if (err.error.message) {
        return err.error.message;
      }

      if (err.error.title) {
        return err.error.title;
      }
    }

    if (err?.message) {
      return err.message;
    }

    return null;
  }
}
