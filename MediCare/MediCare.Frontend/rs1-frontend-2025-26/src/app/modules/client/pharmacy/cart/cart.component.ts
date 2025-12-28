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

  quantities: { [medicineId: number]: number } = {};

  constructor() {
    super();
    this.request = {}; // nema filtera za korpu
  }

  ngOnInit(): void {
    this.loadPagedData();

    // Dark mode
    const darkMode = localStorage.getItem('darkMode');
    if (darkMode === 'true') {
      document.body.classList.add('dark-mode');
    }
  }

  protected loadPagedData(): void {
    this.startLoading();
    this.api.getUserCart().subscribe({
      next: (res: UserCartDto) => {
         console.log('Cart items from backend:', res.items);
        this.items = res.items;

        // inicijalizacija količina
        this.items.forEach(item => {
          this.quantities[item.medicineId] = item.quantity;
        });

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
      console.log('FCM token (test):', res.FcmToken);

      this.toaster.success(`Order placed! Order ID: ${res.orderId}`);
      this.items = [];
      this.quantities = {};
      this.isCheckingOut = false;
    },
    error: (err) => {
      this.toaster.error(`Checkout failed: ${err.error?.message || err.message}`);
      this.isCheckingOut = false;
    }
  });
}




  updateQuantity(cartItem: CartItemDto, newQuantity: number): void {
    if (newQuantity <= 0) return;
    this.quantities[cartItem.medicineId] = newQuantity;
    // opcionalno: update backenda odmah
    const command: AddToCartCommand = {
      medicineId: cartItem.medicineId,
      quantity: newQuantity
    };
    this.api.addToCart(command).subscribe({
      next: () => this.toaster.success('Quantity updated'),
      error: () => this.toaster.error('Failed to update quantity')
    });
  }

removeItem(cartItem: any): void {
  const command: DeleteCartItemCommand = { id: cartItem.cartItemId }; // <--- koristimo cartItemId
  this.api.deleteCartItem(command.id).subscribe({
    next: () => {
      this.items = this.items.filter(i => i.cartItemId !== cartItem.cartItemId);
      this.toaster.success('Item removed from cart');
    },
    error: () => this.toaster.error('Failed to remove item')
  });
}

  getTotal(cartItem: CartItemDto): number {
    return (this.quantities[cartItem.medicineId] || cartItem.quantity) * cartItem.price;
  }
}
