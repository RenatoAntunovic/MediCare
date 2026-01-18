import { Inject, inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CartItemDto,
  UserCartDto,
  AddToCartCommand,
  AddFromFavouritesDto,
  AddFromForLaterDto,
  DeleteCartItemCommand,
  CheckoutOrderResponseDto
} from './carts-api.model';
import { AuthApiService } from '../auth/auth-api.service';

@Injectable({
  providedIn: 'root',
})
export class CartsApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/Cart`; // endpoint za cart
  private http = inject(HttpClient);
  private authService = inject(AuthApiService);

  /**
   * GET /Cart
   * Dohvati cijelu korpu trenutnog korisnika
   */
  getUserCart(): Observable<UserCartDto> {
    return this.http.get<UserCartDto>(this.baseUrl);
  }

  /**
   * POST /Cart
   * Dodaj stavku u korpu
   */
  addToCart(command: AddToCartCommand): Observable<{ cartItemId: number }> {
    return this.http.post<{ cartItemId: number }>(`${this.baseUrl}/items`, command);
  }

  addFromFavourites(dto: AddFromFavouritesDto): Observable<any> {
  return this.http.post(`${this.baseUrl}/add-from-favourites`, dto);
}

    addFromForLater(dto: AddFromForLaterDto): Observable<any> {
  return this.http.post(`${this.baseUrl}/add-from-for-later`, dto);
}

  /**
   * DELETE /Cart/{id}
   * Ukloni stavku iz korpe
   */
  deleteCartItem(id: number): Observable<void> {
  return this.http.delete<void>(`${this.baseUrl}/items/${id}`);
  }

checkout() {
  return this.http.post<CheckoutOrderResponseDto>(
    `${this.baseUrl}/checkout`, {}); // ← Koristi baseUrl kao ostale metode
}

}
