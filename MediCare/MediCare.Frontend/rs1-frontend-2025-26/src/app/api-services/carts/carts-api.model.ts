import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

export interface CartItemDto {
  cartItemId: number;
  cartId: number;
  medicineId: number;
  medicineName: string;
  quantity: number;
  price: number; // cijena po stavci (Medicine.Price * quantity)
  imagePath:string
}

export interface UserCartDto {
  id: number;
  userId: number;
  items: CartItemDto[];
}

export interface AddToCartCommand {
  medicineId: number;
  quantity: number;
}

export interface DeleteCartItemCommand {
  id: number; // cart item ID
}

export interface AddFromFavouritesDto{
  favouriteId:number;
  quantity?:number;
}

export interface AddFromForLaterDto{
  forLaterId:number;
  quantity?:number;
}

export interface CheckoutOrderResponseDto {
  orderId: number;
  totalPrice: number;
  FcmToken: string;
}