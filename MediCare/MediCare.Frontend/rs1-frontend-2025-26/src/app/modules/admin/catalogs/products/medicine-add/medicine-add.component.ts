import {Component, inject, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {MedicineFormService} from '../services/medicine-form.service';
import {CreateMedicineCommand, GetMedicineByIdQueryDto} from '../../../../../api-services/medicine/medicine-api.models';
import {BaseFormComponent} from '../../../../../core/components/base-classes/base-form-component';
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
  selector: 'app-medicine-add',
  standalone: false,
  templateUrl: './medicine-add.component.html',
  styleUrl: './medicine-add.component.scss',
  providers: [MedicineFormService]
})
export class MedicineAddComponent
  extends BaseFormComponent<GetMedicineByIdQueryDto>
  implements OnInit {

  private api = inject(MedicineApiService);
  private categoriesApi = inject(MedicineCategoriesApiService);
  private formService = inject(MedicineFormService);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  selectedImage:File | null=null;

  categories: ListMedicineCategoriesQueryDto[] = [];

  ngOnInit(): void {
    this.initForm(false); // Add mode
    this.loadCategories();
  }

  protected loadData(): void {
    // Not needed in add mode
  }

protected save(): void {
  if (this.form.invalid || this.isLoading) {
    return;
  }

  this.startLoading();

  const formValue = this.form.value;
  const file = this.selectedImage;

  const formData = new FormData();
  formData.append('Name', formValue.name ?? '');
  formData.append('Price', (formValue.price ?? 0).toString());
  formData.append('Description', formValue.description ?? '');
  formData.append('MedicineCategoryId', (formValue.categoryId ?? 0).toString()); // ispravljeno
  formData.append('Weight', (formValue.weight ?? 0).toString());
  formData.append('isEnabled', (formValue.isEnabled ?? false).toString());

  if (this.selectedImage) {
    formData.append('ImageFile',this.selectedImage);
  }

  console.log('FormData entries:');
formData.forEach((value, key) => console.log(key, value));

  console.log('Form data being sent:', formData);
  this.api.create(formData).subscribe({
    next: () => {
      this.stopLoading();
      this.toaster.success('Medicine created successfully');
      this.router.navigate(['/admin/products']);
    },
    error: (err) => {
      this.stopLoading();
      this.toaster.error('Failed to create medicine');
      console.error('Create medicine error:', err);
    }
  });
}



onImageSelected(event: Event): void {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files.length > 0) {
    this.selectedImage = input.files[0];
  } else {
    this.selectedImage = null;
  }
}

  private loadCategories(): void {
    this.categoriesApi.list({ onlyEnabled: true, paging: largePaging }).subscribe({
      next: (response) => {
        this.categories = response.items;
      },
      error: (err) => {
        this.toaster.error('Failed to load categories');
        console.error('Load categories error:', err);
      }
    });
  }

  protected override initForm(isEdit: boolean): void {
    super.initForm(isEdit);
    this.form = this.formService.createMedicineForm();
  }

  onCancel(): void {
    this.router.navigate(['/admin/products']);
  }

  getErrorMessage(controlName: string): string {
    return this.formService.getErrorMessage(this.form, controlName);
  }
}
