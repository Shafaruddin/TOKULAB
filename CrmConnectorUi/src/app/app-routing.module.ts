import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateCaseComponent } from './create-case/create-case.component';
import { AccessGuardService } from './guards/access-guard-service';
import { FrameGuardService } from './guards/frame-guard-service';
import { HomeComponent } from './home/home.component';
import { RedirectComponent } from './redirect/redirect.component';

const routes: Routes = [
  { path: 'admin', loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule), canActivate: [FrameGuardService] },
  { path: 'create-case', component: CreateCaseComponent, canActivate: [AccessGuardService] },
  { path: 'r/:id', component: RedirectComponent, canActivate: [FrameGuardService, AccessGuardService] },
  { path: 'update-case', component: HomeComponent, canActivate: [FrameGuardService, AccessGuardService] },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: false })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
