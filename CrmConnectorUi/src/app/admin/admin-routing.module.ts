import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminComponent } from './admin.component';
import { CacheComponent } from './cache/cache.component';
import { RetailersComponent } from './retailers/retailers.component';
import { StatusFlagsComponent } from './status-flags/status-flags.component';

const routes: Routes = [
  {
    path: ':AdminToken', component: AdminComponent,
    children: [{
      path: 'status-flags', component: StatusFlagsComponent
    },
    {
      path: 'cache', component: CacheComponent
    },
    {
      path: 'retailers', component: RetailersComponent
    }]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
