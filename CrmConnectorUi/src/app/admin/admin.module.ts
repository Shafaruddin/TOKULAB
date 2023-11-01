import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdminRoutingModule } from './admin-routing.module';
import { MomentModule } from 'ngx-moment';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SafePipe } from '../safe.pipe';
import { AdminComponent } from './admin.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { StatusFlagsComponent } from './status-flags/status-flags.component';
import { RetailersComponent } from './retailers/retailers.component';
import { CacheComponent } from './cache/cache.component';
import { UtilsModule } from 'utils/utils.module';

@NgModule({
  declarations: [
    AdminComponent,
    SafePipe,
    StatusFlagsComponent,
    RetailersComponent,
    CacheComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MomentModule,
    NgxDaterangepickerMd.forRoot(),
    AdminRoutingModule,
    UtilsModule
  ]
})
export class AdminModule { }
