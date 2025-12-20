import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TreatmentComponent } from './hospital/threatments/treatments.component';
import { ClientLayoutComponent } from './client-layout/client-layout.component';
import { MedicineComponent } from './pharmacy/medicine/medicine.component';
import { ClientModule } from './client-module';
import { ClientSettingsComponent } from './client-settings/client-settings.component';

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
        path: 'treatments',
        component: TreatmentComponent,
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
