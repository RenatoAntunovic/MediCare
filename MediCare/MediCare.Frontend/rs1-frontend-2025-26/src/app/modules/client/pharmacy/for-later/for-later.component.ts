import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ForLaterApiService } from '../../../../api-services/for-later/for-later-api.service';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';
import { ForLaterDto,AddToForLaterCommand,DeleteForLaterCommand } from '../../../../api-services/for-later/for-later-api.model';
import { CartsApiService } from '../../../../api-services/carts/carts-api.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-for-later',
  standalone: false,
  templateUrl: './for-later.component.html',
  styleUrls: ['./for-later.component.scss']
})
export class ForLaterComponent extends BaseListPagedComponent<ForLaterDto, any> implements OnInit {

  private api = inject(ForLaterApiService);
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
    this.api.getForLater().subscribe({
      next: (res: ForLaterDto[]) => {
        console.log('For Later items from backend:', res);
        this.items = res;
        this.stopLoading();
      },
      error: (err) => {
        console.error('Load for later items error:', err);
        this.stopLoading('Failed to load for later items');
      }
    });
  }

addToCart(item: any) {
  const forLaterId = item.id;

  this.cartService.addFromForLater({ forLaterId, quantity: 1 }).subscribe({
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

  removeItem(favItem: ForLaterDto): void {
    this.api.deleteForLater(favItem.id).subscribe({
      next: () => {
        this.items = this.items.filter(i => i.id !== favItem.id);
        this.toaster.success('Item removed from for later');
      },
      error: () => this.toaster.error('Failed to remove item')
    });
  }
}
