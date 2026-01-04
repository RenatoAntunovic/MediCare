import {NgModule} from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import {AdminRoutingModule} from './admin-routing-module';
import {MedicineComponent} from './catalogs/products/medicine.component';
import {MedicineAddComponent} from './catalogs/products/medicine-add/medicine-add.component';
import {MedicineEditComponent} from './catalogs/products/medicine-edit/medicine-edit.component';
import {AdminLayoutComponent} from './admin-layout/admin-layout.component';
import {MedicineCategoriesComponent} from './catalogs/product-categories/medicine-categories.component';
import {MedicineCategoryUpsertComponent} from './catalogs/product-categories/medicine-category-upsert/medicine-category-upsert.component';
import { TreatmentCategoriesComponent } from './services/treatment-categories/treatment-categories.component';
import { TreatmentCategoryUpsertComponent } from './services/treatment-categories/treatment-category-upsert/treatment-category-upsert.component';
import {AdminOrdersComponent} from './orders/admin-orders.component';
import {AdminSettingsComponent} from './admin-settings/admin-settings.component';
import {SharedModule} from '../shared/shared-module';
import { OrderDetailsDialogComponent } from './orders/admin-orders-details-dialog/order-details-dialog.component';
import { ChangeStatusDialogComponent } from './orders/change-status-dialog/change-status-dialog.component';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { TreatmentComponent } from './services/treatments/treatments.component';
import { TreatmentsAddComponent } from './services/treatments/treatments-add/treatments-add.component';
import { TreatmentsEditComponent } from './services/treatments/treatments-edit/treatments-edit.component';

@NgModule({
  declarations: [
    MedicineComponent,
    MedicineAddComponent,
    MedicineEditComponent,
    AdminLayoutComponent,
    MedicineCategoriesComponent,
    MedicineCategoryUpsertComponent,
    TreatmentCategoriesComponent,
    TreatmentCategoryUpsertComponent,
    TreatmentComponent,
    TreatmentsAddComponent,
    TreatmentsEditComponent,
    AdminOrdersComponent,
    AdminSettingsComponent,
    OrderDetailsDialogComponent,
    ChangeStatusDialogComponent,
  ],
  imports: [
    AdminRoutingModule,
    SharedModule,
    ReactiveFormsModule
  ]
})
export class AdminModule { }
