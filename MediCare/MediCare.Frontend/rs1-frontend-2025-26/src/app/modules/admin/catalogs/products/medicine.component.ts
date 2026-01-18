import { Component, inject, OnInit, ViewChild } from '@angular/core';
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
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MedicineCategoriesApiService } from '../../../../api-services/medicine-categories/medicine-categories-api.service';
import { ListMedicineCategoriesQueryDto } from '../../../../api-services/medicine-categories/medicine-categories-api.model';
import { largePaging } from '../../../../core/models/paging/paging-utils';


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
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private dialogHelper = inject(DialogHelperService);
  private fb = inject(FormBuilder);
  private categoriesApi = inject(MedicineCategoriesApiService);

  displayedColumns: string[] = [
    'imageFile',
    'name',
    'medicineCategoryName',
    'price',
    'weight',
    'isEnabled',
    'actions'
  ];
  dataSource = new MatTableDataSource<any>([]);

  @ViewChild(MatSort) sort!: MatSort;

  // ==================== INLINE EDITING ====================
  editingRowId: number | null = null;
  editForm: FormGroup | null = null;
  categories: ListMedicineCategoriesQueryDto[] = [];
  // ===============================================

  private lastRequestTime = 0;
  private requestCooldown = 2000;

  constructor() {
    super();
    this.request = new ListMedicineRequest();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

 ngOnInit(): void {
  this.loadCategories();
  // loadPagedData će se pozvati kada su kategorije sprema
  this.initList();
}

loadCategories(): void {
  this.categoriesApi.list({ onlyEnabled: true, paging: largePaging }).subscribe({
    next: (response) => {
      this.categories = response.items;
      // Sada učitaj podatke nakon što su kategorije dostupne
      if (!this.items || this.items.length === 0) {
        this.loadPagedData();
      }
    },
    error: (err) => {
      console.error('Failed to load categories:', err);
    }
  });
}


  protected loadPagedData(): void {
    this.startLoading();

    this.api.list(this.request).subscribe({
      next: (response) => {
  console.log('Medicines sa kategorijama:', response.items);
  response.items.forEach(m => {
    console.log(`${m.name} -> categoryName: "${m.medicineCategoryName}"`);
  });
  this.items = response.items;
  this.dataSource.data = this.items;
  this.stopLoading();
},

      error: (err) => {
        console.error('Load error:', err);
        this.stopLoading('Failed to load medicines');
      }
    });
  }

  // === UI Actions ===

  onCreate(): void {
    this.router.navigate(['/admin/products/add']);
  }

  onEdit(medicine: ListMedicineQueryDto): void {
    this.router.navigate(['/admin/products', medicine.id, 'edit']);
  }

  onDelete(medicine: ListMedicineQueryDto): void {
    this.dialogHelper.medicine.confirmDelete(medicine.name).subscribe(result => {
      if (result && result.button === DialogButton.DELETE) {
        this.performDelete(medicine);
      }
    });
  }

  onToggleStatus(medicine: ListMedicineQueryDto): void {
    if (this.isLoading) return;

    this.startLoading();

    const request$ = medicine.isEnabled
      ? this.api.disable(medicine.id)
      : this.api.enable(medicine.id);

    request$.subscribe({
      next: () => {
        medicine.isEnabled = !medicine.isEnabled;
        this.stopLoading();
      },
      error: (err) => {
        console.error('Toggle status error:', err);
        this.stopLoading();
        this.toaster.error('Failed to update status');
      }
    });
  }

  private performDelete(medicine: ListMedicineQueryDto): void {
    this.startLoading();

    this.api.delete(medicine.id).subscribe({
      next: () => {
        this.dialogHelper.medicine.showDeleteSuccess().subscribe();
        this.loadPagedData();
      },
      error: (err) => {
        this.stopLoading();

        this.dialogHelper.showError(
          'DIALOGS.TITLES.ERROR',
          'MEDICINE.DIALOGS.ERROR_DELETE'
        ).subscribe();

        console.error('Delete medicine error:', err);
      }
    });
  }

  onSearch(): void {
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  // ==================== INLINE EDITING ====================

  startEditing(medicine: ListMedicineQueryDto): void {
  this.editingRowId = medicine.id;

  // Kreiraj form sa podacima iz reda
  this.editForm = this.fb.group({
    name: [
      medicine.name,
      [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(150)
      ]
    ],
    price: [
      medicine.price,
      [
        Validators.required,
        Validators.min(0.01)
      ]
    ],
    weight: [
      medicine.weight,
      [
        Validators.required,
        Validators.min(1)
      ]
    ],
    medicineCategoryId: [
      medicine.medicineCategoryId,
      [Validators.required]
    ]
  });
}


  saveRow(medicine: ListMedicineQueryDto): void {
    if (!this.editForm || this.editForm.invalid) {
      this.toaster.error('Molim ispravite greške');
      return;
    }

    this.isLoading = true;
    const formValue = this.editForm.value;

    
    const formData = new FormData();
    formData.append('id', medicine.id.toString());
    formData.append('name', formValue.name);
    formData.append('description', medicine.name); 
    formData.append('price', formValue.price);
    formData.append('MedicineCategoryId', formValue.medicineCategoryId);
    formData.append('weight', formValue.weight);
    formData.append('isEnabled', medicine.isEnabled.toString());
    formData.append('ImageFile', ''); //Bez promjene slike zbog komplikacije putanja fajlova

    this.api.updateFormData(medicine.id, formData).subscribe({
      next: () => {
        
        medicine.name = formValue.name;
        medicine.price = formValue.price;
        medicine.weight = formValue.weight;
        medicine.medicineCategoryId = formValue.medicineCategoryId;
        
        
        const category = this.categories.find(c => c.id === formValue.medicineCategoryId);
        if (category) {
          medicine.medicineCategoryName = category.name;
        }


        this.toaster.success('Lijek uspješno ažuriran');
        this.cancelEditing();
        this.isLoading = false;
      },
      error: (err) => {
        this.toaster.error('Greška pri ažuriranju lijeka');
        console.error('Update error:', err);
        this.isLoading = false;
      }
    });
  }

  cancelEditing(): void {
    this.editingRowId = null;
    this.editForm = null;
  }

  isEditing(medicineId: number): boolean {
    return this.editingRowId === medicineId;
  }

  hasError(controlName: string): boolean {
    if (!this.editForm) return false;
    const control = this.editForm.get(controlName);
    return !!(control && control.invalid && control.touched);
  }

  getErrorMessage(controlName: string): string {
    if (!this.editForm) return '';
    const control = this.editForm.get(controlName);
    if (!control || !control.errors) return '';

    if (control.errors['required']) return `Obavezna polja`;
    if (control.errors['minlength']) return `Min ${control.errors['minlength'].requiredLength} karaktera`;
    if (control.errors['maxlength']) return `Max ${control.errors['maxlength'].requiredLength} karaktera`;
    if (control.errors['min']) return `Min vrijednost ${control.errors['min'].min}`;
    
    return 'Neispravna vrijednost';
  }

 getFormControl(controlName: string) {
  return this.editForm?.get(controlName) as any;
}


}
