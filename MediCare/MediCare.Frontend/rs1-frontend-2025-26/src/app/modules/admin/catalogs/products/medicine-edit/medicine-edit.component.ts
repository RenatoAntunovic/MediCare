import {Component, inject, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {forkJoin} from 'rxjs';
import {MedicineFormService} from '../services/medicine-form.service';
import {BaseFormComponent} from '../../../../../core/components/base-classes/base-form-component';
import {GetMedicineByIdQueryDto, UpdateMedicineCommand} from '../../../../../api-services/medicine/medicine-api.models';
import {MedicineApiService} from '../../../../../api-services/medicine/medicine-api.service';
import {
  MedicineCategoriesApiService
} from '../../../../../api-services/medicine-categories/medicine-categories-api.service';
import {ToasterService} from '../../../../../core/services/toaster.service';
import {
  ListMedicineCategoriesQueryDto
} from '../../../../../api-services/medicine-categories/medicine-categories-api.model';
import {largePaging} from '../../../../../core/models/paging/paging-utils';


@Component({
  selector: 'app-medicine-edit',
  standalone: false,
  templateUrl: './medicine-edit.component.html',
  styleUrl: './medicine-edit.component.scss',
  providers: [MedicineFormService]
})
export class MedicineEditComponent
  extends BaseFormComponent<GetMedicineByIdQueryDto>
  implements OnInit {

  private api = inject(MedicineApiService);
  private categoriesApi = inject(MedicineCategoriesApiService);
  private formService = inject(MedicineFormService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  medicineId!: number;
  categories: ListMedicineCategoriesQueryDto[] = [];

  ngOnInit(): void {
    this.medicineId = +this.route.snapshot.params['id'];
    this.initForm(true); // Edit mode
  }

    selectedFile?: File;
    originalImageFile:File | null=null;

    onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

get currentImageSrc(): string | null {
  if (this.model && this.model.imagePath) {
    return `https://localhost:7260/${this.model.imagePath}`;
  }
  return null;
}

  protected loadData(): void {
    this.startLoading();

    // Load product and categories in parallel
    forkJoin({
      medicine: this.api.getById(this.medicineId),
      categories: this.categoriesApi.list({ onlyEnabled: true, paging: largePaging })
    }).subscribe({
      next: ({ medicine, categories }) => {
        this.model = medicine;
        this.categories = categories.items;
        
        this.form = this.formService.createMedicineForm(medicine);
        this.originalImageFile = this.model?.imagePath ? new File([], this.model.imagePath) : null;

        this.stopLoading();
      },
      error: (err) => {
        this.stopLoading('Failed to load medicine');
        this.toaster.error('Medicine not found');
        console.error('Load medicine error:', err);
        this.router.navigate(['/admin/products']);
      }
    });
  }

protected save(): void {
  if (this.form.invalid || this.isLoading) return;

  this.startLoading();

  const formValue = this.form.value;
  const formData = new FormData();


  formData.append('id', this.medicineId.toString());
  formData.append('name', formValue.name || this.model?.name);
  formData.append('description', formValue.description || this.model?.description);
  formData.append('price', (formValue.price ?? this.model?.price).toString());
  formData.append('MedicineCategoryId', (formValue.medicineCategoryId ?? this.model?.medicineCategoryId).toString());
  formData.append('weight', (formValue.weight ?? this.model?.weight).toString());

if (this.selectedFile) {
  formData.append('ImageFile', this.selectedFile);
} else if (this.originalImageFile) {
  // šalje originalnu sliku ako korisnik nije promijenio
  formData.append('ImageFile', this.originalImageFile);
} else {
  // ako backend dopušta, pošalji prazno polje
  formData.append('ImageFile', '');
}

  this.api.updateFormData(this.medicineId, formData).subscribe({
    next: () => {
      this.stopLoading();
      this.toaster.success('Medicine updated successfully');
      this.router.navigate(['/admin/products']);
    },
    error: (err) => {
      this.stopLoading();
      this.toaster.error('Failed to update medicine');
      console.error('Update medicine error:', err);
    }
  });
}



  onCancel(): void {
    this.router.navigate(['/admin/products']);
  }

  getErrorMessage(controlName: string): string {
    return this.formService.getErrorMessage(this.form, controlName);
  }
}
