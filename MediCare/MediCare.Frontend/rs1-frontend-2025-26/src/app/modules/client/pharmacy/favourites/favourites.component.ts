import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FavouritesService } from '../../../../api-services/favourites/favourites-api.service';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';
import { FavouritesDto, AddToFavouritesCommand, DeleteFavouritesCommand } from '../../../../api-services/favourites/favourites-api.model';
import { CartsApiService } from '../../../../api-services/carts/carts-api.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-favourites',
  standalone: false,
  templateUrl: './favourites.component.html',
  styleUrls: ['./favourites.component.scss']
})
export class FavouritesComponent extends BaseListPagedComponent<FavouritesDto, any> implements OnInit {

  private api = inject(FavouritesService);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private dialogHelper = inject(DialogHelperService);
  private cartService = inject(CartsApiService);
  private snackbar = inject(MatSnackBar);

  displayedColumns: string[] = [
    'imageFile',
    'name',
    'price',
    'actions'
  ];

  constructor() {
    super();
    this.request = {}; // nema filtera
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
    this.api.getFavourites().subscribe({
      next: (res: FavouritesDto[]) => {
        console.log('Favourite items from backend:', res);
        this.items = res;
        this.stopLoading();
      },
      error: (err) => {
        console.error('Load favourites error:', err);
        this.stopLoading('Failed to load favourites');
      }
    });
  }

  get totalPrice(): number {
    if (!this.items) return 0;
    return this.items.reduce((sum, item) => sum + item.price, 0);
  }

addToCart(item: any) {
  const favouriteId = item.id;

  this.cartService.addFromFavourites({ favouriteId, quantity: 1 }).subscribe({
    next: (res: { message: string }) => {
      this.snackbar.open(res.message, '', {
        duration: 2000,
        panelClass: ['custom-snackbar']
      });
    },
    error: () => {
      this.snackbar.open('Greška prilikom dodavanja', '', {
        duration: 2000,
        panelClass: ['custom-snackbar']
      });
    }
  });
}



  removeItem(favItem: FavouritesDto): void {
    this.api.deleteFavourites(favItem.id).subscribe({
      next: () => {
        this.items = this.items.filter(i => i.id !== favItem.id);
        this.toaster.success('Item removed from favourites');
      },
      error: () => this.toaster.error('Failed to remove item')
    });
  }
}
