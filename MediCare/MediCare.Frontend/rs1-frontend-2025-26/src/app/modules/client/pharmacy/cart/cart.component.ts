import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CartsApiService } from '../../../../api-services/carts/carts-api.service';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';
import { CartItemDto, UserCartDto, AddToCartCommand, DeleteCartItemCommand,CheckoutOrderResponseDto } from '../../../../api-services/carts/carts-api.model';

@Component({
  selector: 'app-cart',
  standalone: false,
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss']
})
export class CartComponent extends BaseListPagedComponent<CartItemDto, any> implements OnInit {

  private api = inject(CartsApiService);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private dialogHelper = inject(DialogHelperService);
  isCheckingOut: boolean = false;
  checkoutResponse?: CheckoutOrderResponseDto;


  displayedColumns: string[] = [
    'imageFile',
    'name',
    'price',
    'quantity',
    'actions'
  ];

    /** Cart item whose quantity is being saved (its buttons are disabled meanwhile) */
  updatingItemId: number | null = null;

  constructor() {
    super();
    this.request = {}; // no filters for the cart
  }

  ngOnInit(): void {
    this.loadPagedData();
  }

  protected loadPagedData(): void {
    this.startLoading();
    this.api.getUserCart().subscribe({
      next: (res: UserCartDto) => {
        this.items = res.items;

        this.stopLoading();
      },
      error: (err) => {
        console.error('Load cart error:', err);
        this.stopLoading('Failed to load cart');
      }
    });
  }

get cartTotal(): number {
  if (!this.items) return 0;
  return this.items.reduce((sum, item) => sum + item.price, 0);
}

checkout(): void {
  if (!this.items || this.items.length === 0) {
    this.toaster.error('Your cart is empty');
    return;
  }

  this.isCheckingOut = true;

  this.api.checkout().subscribe({
    next: (res) => {
      this.checkoutResponse = res;

       console.log('Checkout response:', res);

      this.toaster.success(`Order placed! Order ID: ${res.orderId}`);
      this.items = [];
      this.isCheckingOut = false;
    },
    error: (err) => {
      this.toaster.error(`Checkout failed: ${err.error?.message || err.message}`);
      this.isCheckingOut = false;
    }
  });
}

removeItem(cartItem: any): void {
  const command: DeleteCartItemCommand = { id: cartItem.cartItemId }; // <--- use cartItemId
  this.api.deleteCartItem(command.id).subscribe({
    next: () => {
      this.items = this.items.filter(i => i.cartItemId !== cartItem.cartItemId);
      this.toaster.success('Item removed from cart');
    },
    error: () => this.toaster.error('Failed to remove item')
  });
}

  /** − / + buttons: change the quantity by one and update the row with the server's result */
  changeQuantity(item: CartItemDto, delta: number): void {
    const newQuantity = item.quantity + delta;
    if (newQuantity < 1 || newQuantity > 100 || this.updatingItemId !== null) return;

    this.updatingItemId = item.cartItemId;

    this.api.setQuantity(item.cartItemId, newQuantity).subscribe({
      next: result => {
        item.quantity = result.quantity;
        item.price = result.price;
        this.updatingItemId = null;
      },
      error: err => {
        this.updatingItemId = null;
        if (err.status === 429) return; // message already shown by the rate-limit interceptor
        this.toaster.error(err.error?.message || 'Greška pri promjeni količine');
      }
    });
  }

}
