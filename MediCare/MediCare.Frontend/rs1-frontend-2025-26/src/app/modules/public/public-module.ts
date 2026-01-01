import {NgModule} from '@angular/core';

import {PublicRoutingModule} from './public-routing-module';
import {PublicLayoutComponent} from './public-layout/public-layout.component';
import {SearchMedicineComponent} from './search-products/search-medicine.component';
import {SharedModule} from '../shared/shared-module';


@NgModule({
  declarations: [
    PublicLayoutComponent 
    
  ],
  imports: [
    SharedModule,
    PublicRoutingModule,
    SearchMedicineComponent
  ]
})
export class PublicModule { }
