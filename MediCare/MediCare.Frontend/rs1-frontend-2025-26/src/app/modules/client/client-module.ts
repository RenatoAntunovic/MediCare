import {NgModule} from '@angular/core';

import {ClientRoutingModule} from './client-routing-module';
import { MedicineComponent } from './pharmacy/medicine/medicine.component';
import { TreatmentComponent } from './hospital/threatments/treatments.component';
import { ClientLayoutComponent } from './client-layout/client-layout.component';
import { ClientSettingsComponent } from './client-settings/client-settings.component';
import {SharedModule} from '../shared/shared-module';


@NgModule({
  declarations: [
    MedicineComponent,
    ClientLayoutComponent,
    ClientSettingsComponent,
    TreatmentComponent
  ],
  imports: [
    SharedModule,
    ClientRoutingModule
  ]
})
export class ClientModule { }
