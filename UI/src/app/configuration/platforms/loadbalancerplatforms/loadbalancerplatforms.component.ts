import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { DatatableComponent } from '@swimlane/ngx-datatable';
import {
  FormGroup,
  FormBuilder,
  FormControl,
  Validators,
} from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { LoadBalancerPlatformService } from 'src/app/core/service/loadbalancerplatform.service';
import { LoadBalancerPlatformCRUDModel, LoadBalancerPlatformDatabaseModel } from 'src/app/core/models/loadbalancerplatformModel';
import { debounceTime } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'loadbalancerplatforms-outlet',
  templateUrl: './loadbalancerplatforms.component.html',
  styleUrls: ['./loadbalancerplatforms.component.scss'],
})
export class LoadBalancerPlatformComponent implements OnInit {
  loadingIndicator: boolean = true;
  @ViewChild('roleTemplate', { static: true }) roleTemplate: TemplateRef<any>;
  @ViewChild(DatatableComponent, { static: false }) table: DatatableComponent;

  loadbalancerplatformService: LoadBalancerPlatformService;
  loadbalancerplatformCRUDRecord: LoadBalancerPlatformCRUDModel;
  loadbalancerplatformDataRecord: LoadBalancerPlatformDatabaseModel;

  rows = [];
  selectedRowData: LoadBalancerPlatformDatabaseModel;
  loadbalancerplatformsData = [];
  filteredData = [];
  createLoadBalancerPlatformFG: FormGroup;
  updateLoadBalancerPlatformFG: FormGroup;
  selectedOption: string;
  columns = [
    { name: 'LoadBalancerPlatforms' }
  ];
  
  constructor(private _loadbalancerplatformService: LoadBalancerPlatformService, private fb: FormBuilder, private _snackBar: MatSnackBar, private modalService: NgbModal) {
    this.loadbalancerplatformService = _loadbalancerplatformService;
    this.createLoadBalancerPlatformFG = this.fb.group({
      id: new FormControl(),
      platform: new FormControl(),
      location: new FormControl(),
      url_endpoint: new FormControl(),
      verify_ssl_cert: new FormControl(),
    });
    
    this.updateLoadBalancerPlatformFG = this.fb.group({
      id: new FormControl(),
      platform: new FormControl(),
      location: new FormControl(),
      url_endpoint: new FormControl(),
      verify_ssl_cert: new FormControl(),
      row_version: new FormControl(),
    });
  }
  ngOnInit() {
    this.loadData();
 
    this.createLoadBalancerPlatformFG = this.fb.group({
      id: [''],
      row_version: [''],
      platform: ['', [Validators.required]],
      location: ['', [Validators.required]],
      url_endpoint: ['', [Validators.required]],
      verify_ssl_cert: ['']

    });
    this.updateLoadBalancerPlatformFG = this.fb.group({
      id: [''],
      row_version: [''],
      platform: ['', [Validators.required]],
      location: ['', [Validators.required]],
      url_endpoint: ['', [Validators.required]],
      verify_ssl_cert: ['']
    });
    
  }
  loadData() {
    this.fetch((data) => {
      this.loadbalancerplatformsData = data;
      this.filteredData = data;
    });
  }
  fetch(cb) {
    this.loadingIndicator = true;
    this.loadbalancerplatformService.list().subscribe({ 
      next: (loadbalancerplatforms) => {
        this.loadbalancerplatformsData = loadbalancerplatforms;
        this.filteredData = loadbalancerplatforms;
      }, 
      error: (error) => {
        this.showNotification(
          'bg-red',
          `LoadBalancerPlatform list load failed ${error}`,
          'bottom',
          'right'
        );
        this.loadingIndicator = false; 
      },
      complete: () =>  { this.loadingIndicator = false;    }
    });
  }
  // Table 2

  EditRecord(row, content) {
    this.loadingIndicator = true;
    var loadbalancerplatformRecord: LoadBalancerPlatformDatabaseModel;
    this.loadbalancerplatformService.getByID(row.id).subscribe({next: (record) => {
      loadbalancerplatformRecord = record;
      console.log(loadbalancerplatformRecord);
      this.updateLoadBalancerPlatformFG.setValue({
        id: loadbalancerplatformRecord.id,
        platform: loadbalancerplatformRecord.platform,
        location: loadbalancerplatformRecord.location,
        row_version: loadbalancerplatformRecord.row_version,
        url_endpoint: loadbalancerplatformRecord.url_endpoint,
        verify_ssl_cert: loadbalancerplatformRecord.verify_ssl_cert
      });
      this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
      this.selectedRowData = loadbalancerplatformRecord;
    },
    complete: () => {
      this.loadingIndicator = false;
    }});
  }
  AddRecord(content) {
    this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
    this.createLoadBalancerPlatformFG.patchValue({
      
    });
  }
  DeleteRecord(row) {
    this.loadingIndicator = true;
    this.loadbalancerplatformService.delete(row.id).subscribe({ 
      next: (newRecord) => {
        this.showNotification(
          'bg-green',
          'Deleted Record Successfully',
          'bottom',
          'right'
        );
        this.fetch((data) => {
          this.loadbalancerplatformsData = data;
          this.filteredData = data;
        });
       }, 
       error: (error) => {
        this.showNotification(
          'bg-red',
          `LoadBalancerPlatform delete failed: ${error}`,
          'bottom',
          'right'
        );
        this.loadingIndicator = false; 
       },complete: () => {    this.loadingIndicator = false;
       }
      });
  }
  onEditSave(form: FormGroup) {
    this.loadingIndicator = true;
    var updateLoadBalancerPlatform: LoadBalancerPlatformCRUDModel = this.updateLoadBalancerPlatformFG.getRawValue();
    this.loadbalancerplatformService.update(updateLoadBalancerPlatform, this.updateLoadBalancerPlatformFG.get("id").value).subscribe({ 
      next: (newRecord) => {
        form.reset();
        this.modalService.dismissAll();
        this.showNotification(
          'bg-green',
          'Update Record Successfully',
          'bottom',
          'right'
        );
        this.fetch((data) => {
          this.loadbalancerplatformsData = data;
          this.filteredData = data;
        });
       }, 
       error: (error) => {
        this.showNotification(
          'bg-red',
          `LoadBalancerPlatform update failed: ${error}`,
          'bottom',
          'right'
        );
        this.loadingIndicator = false; 
      },complete: () => {    
        this.loadingIndicator = false;}
      });

    this.loadbalancerplatformsData = this.loadbalancerplatformsData.filter((value, key) => {
      if (value.id == form.value.id) {
        value.loadbalancerplatform = form.value.loadbalancerplatform;
        value.password = form.value.password;
        value.username = form.value.username;
        value.row_version = form.value.row_version;
      }
      this.modalService.dismissAll();
      return true;
    });
    this.showNotification(
      'bg-black',
      'Edit Record Successfully',
      'bottom',
      'right'
    );
  }
  onAddRecordSave(form: FormGroup) {
    this.loadingIndicator = true;
    var newLoadBalancerPlatform: LoadBalancerPlatformCRUDModel = this.createLoadBalancerPlatformFG.getRawValue();
    console.log(newLoadBalancerPlatform);
    this.loadbalancerplatformService.create(newLoadBalancerPlatform).subscribe({ 
      next: (newRecord) => {
        form.reset();
        this.modalService.dismissAll();
        this.showNotification(
          'bg-green',
          'Add Record Successfully',
          'bottom',
          'right'
        );
        this.fetch((data) => {
          this.loadbalancerplatformsData = data;
          this.filteredData = data;
        });
       }, 
       error: (error) => {
        this.showNotification(
          'bg-red',
          `LoadBalancerPlatform create failed ${error}`,
          'bottom',
          'right'
        );
        this.loadingIndicator = false; 
      },complete: () => {    this.loadingIndicator = false;}
      });

  }
  filterDatatable(event) {
    // get the value of the key pressed and make it lowercase
    const val = event.target.value.toLowerCase();
    // get the amount of columns in the table
    const colsAmt = this.columns.length;
    // get the key names of each column in the dataset
    const keys = Object.keys(this.filteredData[0]);
    // assign filtered matches to the active datatable
    this.loadbalancerplatformsData = this.filteredData.filter(function (item) {
      // iterate through each row's column data
      for (let i = 0; i < colsAmt; i++) {
        // check for a match
        if (
          item[keys[i]].toString().toLowerCase().indexOf(val) !== -1 ||
          !val
        ) {
          // found match, return true to add to result set
          return true;
        }
      }
    });
    // whenever the filter changes, always go back to the first page
    this.table.offset = 0;
  }
  showNotification(colorName, text, placementFrom, placementAlign) {
    this._snackBar.open(text, '', {
      duration: 2000,
      verticalPosition: placementFrom,
      horizontalPosition: placementAlign,
      panelClass: colorName,
    });
  }
}

