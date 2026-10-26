import {NgModule} from '@angular/core';

import {ClientRoutingModule} from './client-routing-module';
import { MedicineComponent } from './pharmacy/medicine/medicine.component';
import { TreatmentComponent } from './hospital/threatments/treatments.component';
import { ClientLayoutComponent } from './client-layout/client-layout.component';
import { ClientSettingsComponent } from './client-settings/client-settings.component';
import {SharedModule} from '../shared/shared-module';
import { MedicineDetailComponent } from './pharmacy/medicine-detail/medicine-detail.component';
import { CartComponent } from './pharmacy/cart/cart.component';
import { FavouritesComponent } from './pharmacy/favourites/favourites.component';
import { ForLaterComponent } from './pharmacy/for-later/for-later.component';
import { OrderDetailsClientDialogComponent } from './orders/client-order-details-dialog/order-details-dialog.component';
import { ClientOrdersComponent } from './orders/client-orders.component';
import { ClientAssistantComponent } from './assistant/client-assistant.component';
import { AssistantChatComponent } from '../shared/components/assistant-chat/assistant-chat.component';

@NgModule({
  declarations: [
    MedicineComponent,
    MedicineDetailComponent,
    ClientLayoutComponent,
    ClientSettingsComponent,
    TreatmentComponent,
    CartComponent,
    FavouritesComponent,
    ForLaterComponent,
    OrderDetailsClientDialogComponent,
    ClientOrdersComponent,
    ClientAssistantComponent
  ],
  imports: [
    SharedModule,
    ClientRoutingModule,
    AssistantChatComponent
  ]
})
export class ClientModule { }
