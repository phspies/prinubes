import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { IdentityComponent } from './identity.component';
import { CredentialComponent } from './credentials/credentials.component';
import { IdentityRoutingModule } from './identity-routing.module';
import { MatInputModule } from '@angular/material/input';
@NgModule({
  declarations: [IdentityComponent, CredentialComponent],
  imports: [
    CommonModule,
    IdentityRoutingModule,
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

export class IdentityModule {}
