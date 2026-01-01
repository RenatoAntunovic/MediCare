import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  ListMedicineRequest,
  ListMedicineQueryDto
} from '../../../../api-services/medicine/medicine-api.models';
import { MedicineApiService } from '../../../../api-services/medicine/medicine-api.service';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';
import { DialogButton } from '../../../shared/models/dialog-config.model';
import {
  ListMedicineCategoriesQueryDto,
  ListMedicineCategoriesRequest
} from '../../../../api-services/medicine-categories/medicine-categories-api.model';
import { MedicineCategoriesApiService } from '../../../../api-services/medicine-categories/medicine-categories-api.service';
import { FavouritesService } from '../../../../api-services/favourites/favourites-api.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-medicine',
  standalone: false,
  templateUrl: './medicine.component.html',
  styleUrl: './medicine.component.scss'
})
export class MedicineComponent
  extends BaseListPagedComponent<ListMedicineQueryDto, ListMedicineRequest>
  implements OnInit {

  private api = inject(MedicineApiService);
  private categoryApi = inject(MedicineCategoriesApiService);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private dialogHelper = inject(DialogHelperService);
  private favoritesService = inject(FavouritesService);
  private snackbar = inject(MatSnackBar);

  isDarkMode = false;

  displayedColumns: string[] = [
    'imageFile',
    'name',
    'medicineCategoryName',
    'price',
    'weight',
    'isEnabled',
    'actions'
  ];

  categories: ListMedicineCategoriesQueryDto[] = [];
  categoryFilter: number | null = null;

  constructor() {
    super();
    this.request = new ListMedicineRequest();
  }

  toggleDarkMode(): void {
    this.isDarkMode = !this.isDarkMode;
    document.body.classList.toggle('dark-mode', this.isDarkMode);
    localStorage.setItem('darkMode', this.isDarkMode ? 'true' : 'false');
  }

  goToMedicineDetail(medicine: any) {
    this.router.navigate(['/client/medicine', medicine.id]);
  }

  ngOnInit(): void {
    this.initList();
    this.loadCategories();

    const darkMode = localStorage.getItem('darkMode');
    if (darkMode === 'true') {
      document.body.classList.add('dark-mode');
    }
  }

  onCategoryFilterChange(categoryId: number | null): void {
    this.categoryFilter = categoryId;
    this.request.categoryId = categoryId;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  private loadCategories(): void {
    const request = new ListMedicineCategoriesRequest();
    request.onlyEnabled = true;
    request.paging.page = 1;
    request.paging.pageSize = 100;

    this.categoryApi.list(request).subscribe(res => {
      this.categories = res.items;
    });
  }

  addToFavourites(item: any) {
    const command = { medicineId: item.id };

    this.favoritesService.addToFavourites(command).subscribe({
      next: (res: { favouriteId: number }) => {
        this.snackbar.open('Dodano u favorite', '', {
          duration: 2000,
          panelClass: ['custom-snackbar']
        });
        item.isFavourite = true;
      },
      error: () => {
        this.snackbar.open('Greška prilikom dodavanja u favorite', '', {
          duration: 2000,
          panelClass: ['custom-snackbar']
        });
      }
    });
  }

  onCreate() {}
  onEdit(medicine: any) {}
  onDelete(medicine: any) {}
  onToggleStatus(medicine: any) {}

  // Glavno učitavanje: bez query -> SQL lista, sa query -> Elasticsearch
  protected loadPagedData(): void {
  this.startLoading();

  const query = this.request.search?.trim() ?? '';
  console.log('FRONT QUERY =', query); // DEBUG

  if (!query) {
    // Bez pretrage – klasična lista iz SQL-a
    this.api.list(this.request).subscribe({
      next: (response) => {
        this.items = response.items;
        this.stopLoading();
      },
      error: (err) => {
        console.error('Load error (SQL):', err);
        this.stopLoading('Failed to load medicines');
      }
    });
  } else {
    // Sa pretragom – Elasticsearch (ime + opis + kategorija)
    this.api.searchMedicines(query, this.request.paging.page, this.request.paging.pageSize)
      .subscribe({
        next: (response) => {
          console.log('SEARCH RESPONSE', response); // DEBUG
          this.items = response.results ?? response;
          this.stopLoading();
        },
        error: (err) => {
          console.error('Load error (ES):', err);
          this.stopLoading('Failed to load medicines');
        }
      });
  }
}


  // Enter search
  private lastSearchTime: number = 0;
private searchCooldownMs: number = 500; // 500ms između search-eva

onSearch(): void {
  const now = Date.now();
  const timeSinceLastSearch = now - this.lastSearchTime;

  if (timeSinceLastSearch < this.searchCooldownMs) {
    // Previše zahtjeva - prikaži poruku
    this.toaster.error('Molim vas, sacekajte malo prije nego sto ponovo pretrazujete');
    return;
  }

  this.lastSearchTime = now;
  this.request.paging.page = 1;
  this.loadPagedData(); // ← Direktno pozovi loadPagedData(), bez searchSubject
}


}
