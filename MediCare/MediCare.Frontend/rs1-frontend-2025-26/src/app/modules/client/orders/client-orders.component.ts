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
import { OrderDetailsClientDialogComponent } from './client-order-details-dialog/order-details-dialog.component';

@Component({
  selector: 'app-client-orders',
  standalone: false,
  templateUrl: './client-orders.component.html',
  styleUrl: './client-orders.component.scss',
})
export class ClientOrdersComponent
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

  protected loadPagedData(): void {
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

    const dialogRef = this.dialog.open(OrderDetailsClientDialogComponent, {
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
