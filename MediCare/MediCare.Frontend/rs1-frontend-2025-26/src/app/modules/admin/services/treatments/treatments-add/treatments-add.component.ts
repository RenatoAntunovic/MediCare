import {Component, inject, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {TreatmentsFormService} from '../services/treatments-form.service';
import {CreateTreatmentsCommand, GetTreatmentsByIdQueryDto} from '../../../../../api-services/treatments/treatments-api.models';
import {BaseFormComponent} from '../../../../../core/components/base-classes/base-form-component';
import {TreatmentsApiService} from '../../../../../api-services/treatments/treatments-api.service';
import {TreatmentCategoriesApiService} from '../../../../../api-services/treatment-categories/treatment-categories-api.service';
import {ToasterService} from '../../../../../core/services/toaster.service';
import {
  ListTreatmentCategoriesQueryDto
} from '../../../../../api-services/treatment-categories/treatment-categories-api.model';
import {largePaging} from '../../../../../core/models/paging/paging-utils';

@Component({
  selector: 'app-treatments-add',
  standalone: false,
  templateUrl: './treatments-add.component.html',
  styleUrl: './treatments-add.component.scss',
  providers: [TreatmentsFormService]
})
export class TreatmentsAddComponent
  extends BaseFormComponent<GetTreatmentsByIdQueryDto>
  implements OnInit {

  private api = inject(TreatmentsApiService);
  private categoriesApi = inject(TreatmentCategoriesApiService);
  private formService = inject(TreatmentsFormService);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  selectedImage:File | null=null;

  categories: ListTreatmentCategoriesQueryDto[] = [];

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
  formData.append('ServiceName', formValue.name ?? '');
  formData.append('Price', (formValue.price ?? 0).toString());
  formData.append('Description', formValue.description ?? '');
  formData.append('TreatmentCategoryId', (formValue.categoryId ?? 0).toString()); // ispravljeno
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
      this.toaster.success('Treatment created successfully');
      this.router.navigate(['/admin/treatments']);
    },
    error: (err) => {
      this.stopLoading();
      this.toaster.error('Failed to create treatment');
      console.error('Create treatment error:', err);
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
    this.form = this.formService.createTreatmentsForm();
  }

  onCancel(): void {
    this.router.navigate(['/admin/treatments']);
  }

  getErrorMessage(controlName: string): string {
    return this.formService.getErrorMessage(this.form, controlName);
  }
}
