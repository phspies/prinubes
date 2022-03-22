import { Component, OnInit, ViewChild } from '@angular/core';
import { OrganizationDatabaseModel } from 'src/app/core/models/organizationDatabaseModel';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss'],
})
export class MainComponent implements OnInit {
  public organization: OrganizationDatabaseModel = JSON.parse(localStorage.getItem('currentOrganization'));;
  ngOnInit() {
    
  }
}
