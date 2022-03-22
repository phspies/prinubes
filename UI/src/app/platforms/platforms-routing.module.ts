import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ComputePlatformComponent } from './computeplatforms/computeplatforms.component';
import { PlatformComponent } from './platforms.component';
export const routes: Routes = [
  {
    path: '',
    component: PlatformComponent
  },
  {
    path: 'computeplatforms',
    component: ComputePlatformComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PlatformsRoutingModule {
}

