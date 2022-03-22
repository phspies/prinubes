import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { ComputePlatformComponent } from './computeplatforms/computeplatforms.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { PlatformsRoutingModule } from './platforms-routing.module';
import { PlatformComponent } from './platforms.component';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NetworkPlatformComponent } from './networkplatforms/networkplatforms.component';
import { LoadBalancerPlatformComponent } from './loadbalancerplatforms/loadbalancerplatforms.component';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { MatInputModule } from '@angular/material/input';

@NgModule({
  declarations: [PlatformComponent, ComputePlatformComponent, NetworkPlatformComponent, LoadBalancerPlatformComponent],
  imports: [
    CommonModule,
    PlatformsRoutingModule,
    PerfectScrollbarModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatTabsModule,
    MatCheckboxModule,
    MatFormFieldModule,
    FormsModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    NgxDatatableModule,
    MatInputModule,
    MatSnackBarModule

  ],
})

export class PlatformsModule {}
