
/**
 * Helper class for working with Order Status
 *
 * Provides human-readable labels and styling information
 * for order statuses throughout the application.
 */
export class OrderStatusHelper {

  /**
   * Get human-readable label for order status
   *
   * @param status - Order status enum value
   * @returns Translated label key or default label
   */
  static getLabel(status: { id: number; name: string }): string {
    switch (status.id) {
    case 1: return 'status-draft';
    case 2: return 'status-confirmed';
    case 3: return 'status-paid';
    case 4: return 'status-completed';
    case 5: return 'status-cancelled';
    default: return 'status-unknown';
    }
  }

  /**
   * Get color class for order status badge
   *
   * Use these classes with your badge/chip component
   *
   * @param status - Order status enum value
   * @returns CSS class name for status color
   */
  static getColorClass(status: { id: number; name: string }): string {
    switch (status.id) {
    case 1: return 'status-draft';
    case 2: return 'status-confirmed';
    case 3: return 'status-paid';
    case 4: return 'status-completed';
    case 5: return 'status-cancelled';
    default: return 'status-unknown';
    }
  }

  /**
   * Get Material icon name for order status
   *
   * @param status - Order status enum value
   * @returns Material icon name
   */
static getIcon(status: { id: number; name: string }): string {
  switch (status.id) {
    case 1: return 'edit_note';
    case 2: return 'check_circle';
    case 3: return 'payment';
    case 4: return 'done_all';
    case 5: return 'cancel';
    default: return 'help_outline';
  }
}


  /**
   * Get all available statuses
   *
   * Useful for dropdowns/filters
   *
   * @returns Array of all order statuses
   */
static getAllStatuses(): { id: number; name: string }[] {
  return [
    { id: 1, name: 'Draft' },
    { id: 2, name: 'Confirmed' },
    { id: 3, name: 'Paid' },
    { id: 4, name: 'Completed' },
    { id: 5, name: 'Cancelled' }
  ];
}


  /**
   * Get status options for dropdown
   *
   * @returns Array of status options with label and value
   */
static getStatusOptions(): Array<{ label: string; value: { id: number; name: string }; icon: string }> {
  return this.getAllStatuses().map(status => ({
    label: status.name,         
    value: status,              
    icon: this.getIcon(status)  
  }));
}


  /**
   * Check if status allows editing
   *
   * @param status - Order status enum value
   * @returns true if order can be edited
   */
static canEdit(status: { id: number; name: string }): boolean {
  // Only Draft (id:1) and Confirmed (id:2) orders can be edited
  return status.id === 1 || status.id === 2;
}

static canCancel(status: { id: number; name: string }): boolean {
  // Can cancel Draft (1), Confirmed (2), and Paid (3) orders
  return status.id === 1 || status.id === 2 || status.id === 3;
}


  /**
   * Get next possible status transitions
   *
   * @param currentStatus - Current order status
   * @returns Array of possible next statuses
   */
static getNextStatuses(currentStatus: { id: number; name: string }): { id: number; name: string }[] {
  switch (currentStatus.id) {
    case 1: return [{ id: 2, name: 'Confirmed' }, { id: 5, name: 'Cancelled' }];
    case 2: return [{ id: 3, name: 'Paid' }, { id: 5, name: 'Cancelled' }];
    case 3: return [{ id: 4, name: 'Completed' }, { id: 5, name: 'Cancelled' }];
    default: return [];
  }
}

}
