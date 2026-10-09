import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  FavouritesDto,
  AddToFavouritesCommand,
  DeleteFavouritesCommand
} from './favourites-api.model';

@Injectable({
  providedIn: 'root',
})
export class FavouritesService {
  private readonly baseUrl = `${environment.apiUrl}/api/Favourites`;
  private http = inject(HttpClient);

  /**
   * GET /Cart
   * Get the whole cart of the current user
   */
  getFavourites(): Observable<FavouritesDto[]> {
    return this.http.get<FavouritesDto[]>(this.baseUrl);
  }

  /**
   * POST /Cart
   * Add an item to the cart
   */
  addToFavourites(command: AddToFavouritesCommand): Observable<{ favouriteId: number }> {
    return this.http.post<{ favouriteId: number }>(`${this.baseUrl}`, command);
  }


  /**
   * DELETE /Cart/{id}
   * Remove an item from the cart
   */
  deleteFavourites(id: number): Observable<void> {
  return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

}
