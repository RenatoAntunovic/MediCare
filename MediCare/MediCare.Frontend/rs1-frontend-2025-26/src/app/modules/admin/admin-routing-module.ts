import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { MedicineComponent } from './catalogs/products/medicine.component';
import { MedicineAddComponent } from './catalogs/products/medicine-add/medicine-add.component';
import { MedicineEditComponent } from './catalogs/products/medicine-edit/medicine-edit.component';
import { MedicineCategoriesComponent } from './catalogs/product-categories/medicine-categories.component';
import { TreatmentCategoriesComponent } from './services/treatment-categories/treatment-categories.component';
import {AdminOrdersComponent} from './orders/admin-orders.component';
import {AdminSettingsComponent} from './admin-settings/admin-settings.component';
import { TreatmentComponent } from './services/treatments/treatments.component';
import { TreatmentsAddComponent } from './services/treatments/treatments-add/treatments-add.component';
import { TreatmentsEditComponent } from './services/treatments/treatments-edit/treatments-edit.component';

const routes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      // PRODUCTS
      {
        path: 'products',
        component: MedicineComponent,
      },
      {
        path: 'products/add',
        component: MedicineAddComponent,
      },
      {
        path: 'products/:id/edit',
        component: MedicineEditComponent,
      },

      // PRODUCT CATEGORIES
      {
        path: 'product-categories',
        component: MedicineCategoriesComponent,
      },

      {
        path: 'treatment-categories',
        component: TreatmentCategoriesComponent,
      },
       {
        path: 'treatments',
        component: TreatmentComponent,
      },
      {
        path: 'treatments/add',
        component: TreatmentsAddComponent,
      },
      {
        path: 'treatments/:id/edit',
        component: TreatmentsEditComponent,
      },
      {
        path: 'orders',
        component: AdminOrdersComponent,
      },

      {
        path: 'settings',
        component: AdminSettingsComponent,
      },


      // default admin route → /admin/products
      {
        path: '',
        redirectTo: 'products',
        pathMatch: 'full',
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
