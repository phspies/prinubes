import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CredentialComponent } from './credentials/credentials.component';
import { IdentityComponent } from './identity.component';
export const routes: Routes = [
  {
    path: '',
    component: IdentityComponent
  },
  {
    path: 'credentials',
    component: CredentialComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class IdentityRoutingModule {
}

