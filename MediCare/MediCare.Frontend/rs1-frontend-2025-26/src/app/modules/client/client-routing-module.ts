import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TreatmentComponent } from './hospital/threatments/treatments.component';
import { ClientLayoutComponent } from './client-layout/client-layout.component';
import { MedicineComponent } from './pharmacy/medicine/medicine.component';
import { CartComponent } from './pharmacy/cart/cart.component';
import { ClientModule } from './client-module';
import { ClientSettingsComponent } from './client-settings/client-settings.component';
import { MedicineDetailComponent } from './pharmacy/medicine-detail/medicine-detail.component';
import { FavouritesComponent } from './pharmacy/favourites/favourites.component';
import { ForLaterComponent } from './pharmacy/for-later/for-later.component';
import { OrderDetailsClientDialogComponent } from './orders/client-order-details-dialog/order-details-dialog.component';
import { ClientOrdersComponent } from './orders/client-orders.component';

const routes: Routes = [
  {
    path:'',
    component:ClientLayoutComponent,
    children:[
      {
        path: 'medicine',
        component: MedicineComponent,
      },
      {
        path:'cart',
        component: CartComponent,
      },
         {
        path:'favourites',
        component: FavouritesComponent,
      },
       {
        path:'for-later',
        component: ForLaterComponent,
      },
      {
        path:'medicine/:id', component:MedicineDetailComponent
      },
      {
        path:'', redirectTo:'medicine', pathMatch:'full'
      },
      {
        path: 'treatments',
        component: TreatmentComponent,
      },
            {
        path: 'orders',
        component: ClientOrdersComponent,
      },
      {
        path: 'settings',
        component: ClientSettingsComponent,
      },

      {
        path: '',
        redirectTo: 'hospital',
        pathMatch: 'full',
      },
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ClientRoutingModule { }
