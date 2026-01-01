import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';   // ← OVO
import { FormsModule } from '@angular/forms'; 
import { MedicineApiService } from '../../../api-services/medicine/medicine-api.service';
import { ListMedicineQueryDto } from '../../../api-services/medicine/medicine-api.models';

@Component({
  standalone: true,
  selector: 'app-search-medicine',
  templateUrl: './search-medicine.component.html',
  styleUrls: ['./search-medicine.component.scss'],
  imports: [CommonModule, FormsModule]
})
export class SearchMedicineComponent implements OnInit {

  searchQuery = '';
  medicines: ListMedicineQueryDto[] = [];
  isLoading = false;
  private searchTimeout: any = null;

  // OPCIJA 1 – klasični DI
  constructor(private medicineApiService: MedicineApiService) {}

  // ili OPCIJA 2 – inject:
  // private medicineApiService = inject(MedicineApiService);

  ngOnInit(): void {
    // opcionalno: loadAllMedicines();
  }

  onSearchChange(query: string) {
    console.log('onSearchChange fired', query);
    if (!query || query.trim().length < 2) {
      this.medicines = [];
      return;
    }

    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }

    this.searchTimeout = setTimeout(() => {
      this.isLoading = true;
      this.medicineApiService.searchMedicines(query).subscribe({
        next: (response) => {
          // ako API vraća { results: [...] }
          this.medicines = response.results ?? response;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        }
      });
    }, 300);
  }
}
