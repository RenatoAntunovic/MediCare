import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MedicineApiService } from '../../../../api-services/medicine/medicine-api.service';
import { GetMedicineByIdQueryDto } from '../../../../api-services/medicine/medicine-api.models';
import { CartsApiService } from '../../../../api-services/carts/carts-api.service';
import { FavouritesService } from '../../../../api-services/favourites/favourites-api.service';
import { ForLaterApiService } from '../../../../api-services/for-later/for-later-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { inject } from '@angular/core';
import { Location } from '@angular/common';

@Component({
  selector: 'app-medicine-detail',
  standalone: false,
  templateUrl: './medicine-detail.component.html',
  styleUrls: ['./medicine-detail.component.scss']
})
export class MedicineDetailComponent implements OnInit {
  selectedDose: string = '';
  selectedPackage: string = '';
  errorMessage: string = '';
  isLoading: boolean = true;
  private cartApi = inject(CartsApiService);
  private favApi = inject(FavouritesService);
  private forLaterApi = inject(ForLaterApiService);
  private toaster = inject(ToasterService);
  private location = inject(Location);

  medicine: GetMedicineByIdQueryDto = {
  id: 0,
  name: '',
  description: '',
  price: 0,
  medicineCategoryId: 0,
  medicineCategoryName: '',
  imagePath: '',
  weight: 0,
  isEnabled: false,
};

  constructor(
    private route: ActivatedRoute,
    private medicineService: MedicineApiService
  ) {}
  
  ngOnInit(): void {
    const idStr = this.route.snapshot.paramMap.get('id');
    if (!idStr) {
      this.errorMessage = 'ID nije pronađen u URL-u';
      this.isLoading = false;
      return;
    }

    const id = +idStr;

    this.medicineService.getById(id).subscribe({
      next: (data) => {
         console.log('API response:', data);
        this.medicine = data;

        console.log('Medicine fetched:', this.medicine);

        this.isLoading = false; // ✅ Ovo je ključno
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Greška pri dohvaćanju podataka o lijeku';
        this.isLoading = false;
      }
    });
  }

    goBack(): void  {
    this.location.back();
  }

    quantity = 1;

addToCart() {
  if (!this.medicine || !this.quantity) return;

  this.cartApi.addToCart({
    medicineId: this.medicine.id,
    quantity: this.quantity
  }).subscribe({
    next: (res) => {
      this.toaster.success(`Dodano u košaricu (ID: ${res.cartItemId})`);
    },
    error: (err) => {
      console.error(err);
      this.toaster.error('Greška pri dodavanju u košaricu');
    }
  });
}

addToFavourites() {
  if (!this.medicine) return;

  this.favApi.addToFavourites({
    medicineId: this.medicine.id,
  }).subscribe({
    next: (res) => {
      this.toaster.success(`Dodano u košaricu (ID: ${res.favouriteId})`);
    },
    error: (err) => {
      console.error(err);
      this.toaster.error('Greška pri dodavanju u košaricu');
    }
  });
}

addToForLater() {
  if (!this.medicine) return;

  this.forLaterApi.addToForLater({
    medicineId: this.medicine.id,
  }).subscribe({
    next: (res) => {
      this.toaster.success(`Dodano u košaricu (ID: ${res.forLaterId})`);
    },
    error: (err) => {
      console.error(err);
      this.toaster.error('Greška pri dodavanju u košaricu');
    }
  });
}

}
