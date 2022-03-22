import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { DatatableComponent } from '@swimlane/ngx-datatable';
import {
  FormGroup,
  FormBuilder,
  FormControl,
  Validators,
} from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ComputePlatformService } from 'src/app/core/service/computeplatform.service';
import { ComputePlatformCRUDModel, ComputePlatformDatabaseModel } from 'src/app/core/models/computeplatformModel';
import { debounceTime } from 'rxjs';

@Component({
  selector: 'computeplatforms-outlet',
  templateUrl: './computeplatforms.component.html',
  styleUrls: ['./computeplatforms.component.scss'],
})
export class ComputePlatformComponent implements OnInit {
  loadingIndicator: boolean = true;
  @ViewChild('roleTemplate', { static: true }) roleTemplate: TemplateRef<any>;
  @ViewChild(DatatableComponent, { static: false }) table: DatatableComponent;

  computeplatformService: ComputePlatformService;
  computeplatformCRUDRecord: ComputePlatformCRUDModel;
  computeplatformDataRecord: ComputePlatformDatabaseModel;

  rows = [];
  selectedRowData: ComputePlatformDatabaseModel;
  computeplatformsData = [];
  filteredData = [];
  createComputePlatformFG: FormGroup;
  updateComputePlatformFG: FormGroup;
  selectedOption: string;
  columns = [
    { name: 'ComputePlatforms' }
  ];
  
  constructor(private _computeplatformService: ComputePlatformService, private fb: FormBuilder, private _snackBar: MatSnackBar, private modalService: NgbModal) {
    this.computeplatformService = _computeplatformService;
    this.createComputePlatformFG = this.fb.group({
      id: new FormControl(),
      platform: new FormControl(),
      location: new FormControl(),
      url_endpoint: new FormControl(),
      verify_ssl_cert: new FormControl(),
    });
    
    this.updateComputePlatformFG = this.fb.group({
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
 
    this.createComputePlatformFG = this.fb.group({
      id: [''],
      row_version: [''],
      platform: ['', [Validators.required]],
      location: ['', [Validators.required]],
      url_endpoint: ['', [Validators.required]],
      verify_ssl_cert: ['']

    });
    this.updateComputePlatformFG = this.fb.group({
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
      this.computeplatformsData = data;
      this.filteredData = data;
    });
  }
  fetch(cb) {
    this.loadingIndicator = true;
    this.computeplatformService.list().subscribe({ 
      next: (computeplatforms) => {
        this.computeplatformsData = computeplatforms;
        this.filteredData = computeplatforms;
      }, 
      error: (error) => {
        this.showNotification(
          'bg-red',
          `ComputePlatform list load failed ${error}`,
          'bottom',
          'right'
        );
      },
      complete: () =>  { this.loadingIndicator = false;    }
    });
  }
  // Table 2

  EditRecord(row, content) {
    this.loadingIndicator = true;
    var computeplatformRecord: ComputePlatformDatabaseModel;
    this.computeplatformService.getByID(row.id).subscribe({next: (record) => {
      computeplatformRecord = record;
      console.log(computeplatformRecord);
      this.updateComputePlatformFG.setValue({
        id: computeplatformRecord.id,
        platform: computeplatformRecord.platform,
        location: computeplatformRecord.location,
        row_version: computeplatformRecord.row_version,
        url_endpoint: computeplatformRecord.url_endpoint,
        verify_ssl_cert: computeplatformRecord.verify_ssl_cert
      });
      this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
      this.selectedRowData = computeplatformRecord;
    },
    complete: () => {
      this.loadingIndicator = false;
    }});
  }
  AddRecord(content) {
    this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
    this.createComputePlatformFG.patchValue({
      
    });
  }
  DeleteRecord(row) {
    this.loadingIndicator = true;
    this.computeplatformService.delete(row.id).subscribe({ 
      next: (newRecord) => {
        this.showNotification(
          'bg-green',
          'Deleted Record Successfully',
          'bottom',
          'right'
        );
        this.fetch((data) => {
          this.computeplatformsData = data;
          this.filteredData = data;
        });
       }, 
       error: (error) => {
        this.showNotification(
          'bg-red',
          `ComputePlatform delete failed: ${error}`,
          'bottom',
          'right'
        );
       },complete: () => {    this.loadingIndicator = false;
       }
      });
  }
  onEditSave(form: FormGroup) {
    this.loadingIndicator = true;
    var updateComputePlatform: ComputePlatformCRUDModel = this.updateComputePlatformFG.getRawValue();
    this.computeplatformService.update(updateComputePlatform, this.updateComputePlatformFG.get("id").value).subscribe({ 
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
          this.computeplatformsData = data;
          this.filteredData = data;
        });
       }, 
       error: (error) => {
        this.showNotification(
          'bg-red',
          `ComputePlatform update failed: ${error}`,
          'bottom',
          'right'
        )
      },complete: () => {    
        this.loadingIndicator = false;}
      });

    this.computeplatformsData = this.computeplatformsData.filter((value, key) => {
      if (value.id == form.value.id) {
        value.computeplatform = form.value.computeplatform;
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
    var newComputePlatform: ComputePlatformCRUDModel = this.createComputePlatformFG.getRawValue();
    console.log(newComputePlatform);
    this.computeplatformService.create(newComputePlatform).subscribe({ 
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
          this.computeplatformsData = data;
          this.filteredData = data;
        });
       }, 
       error: (error) => {
        this.showNotification(
          'bg-red',
          `ComputePlatform create failed ${error}`,
          'bottom',
          'right'
        );
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
    this.computeplatformsData = this.filteredData.filter(function (item) {
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

