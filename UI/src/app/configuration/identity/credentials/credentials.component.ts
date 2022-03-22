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
import { CredentialService } from 'src/app/core/service/credential.service';
import { CredentialCRUDModel, CredentialDatabaseModel } from 'src/app/core/models/credentialModels';
import { debounceTime } from 'rxjs';

@Component({
  selector: 'credentials-outlet',
  templateUrl: './credentials.component.html',
  styleUrls: ['./credentials.component.scss'],
})
export class CredentialComponent implements OnInit {
  loadingIndicator: boolean = true;
  @ViewChild('roleTemplate', { static: true }) roleTemplate: TemplateRef<any>;
  @ViewChild(DatatableComponent, { static: false }) table: DatatableComponent;

  credentialService: CredentialService;
  credentialCRUDRecord: CredentialCRUDModel;
  credentialDataRecord: CredentialDatabaseModel;

  rows = [];
  selectedRowData: CredentialDatabaseModel;
  credentialsData = [];
  filteredData = [];
  createCredentialFG: FormGroup;
  updateCredentialFG: FormGroup;
  selectedOption: string;
  columns = [
    { name: 'Credentials' }
  ];
  
  constructor(private _credentialService: CredentialService, private fb: FormBuilder, private _snackBar: MatSnackBar, private modalService: NgbModal) {
    this.credentialService = _credentialService;
    this.createCredentialFG = this.fb.group({
      id: new FormControl(),
      credential: new FormControl(),
      username: new FormControl(),
      password: new FormControl(),
      row_version: new FormControl(),
    });
    
    this.updateCredentialFG = this.fb.group({
      id: new FormControl(),
      credential: new FormControl(),
      username: new FormControl(),
      password: new FormControl(),
      row_version: new FormControl(),
    });
  }
  ngOnInit() {
    this.fetch((data) => {
      this.credentialsData = data;
      this.filteredData = data;
    });
 
    this.createCredentialFG = this.fb.group({
      id: [''],
      row_version: [''],
      credential: ['', [Validators.required]],
      username: ['', [Validators.required]],
      password: ['', [Validators.required]],
    });
    this.updateCredentialFG = this.fb.group({
      id: [''],
      row_version: [''],
      credential: ['', [Validators.required]],
      username: ['', [Validators.required]],
      password: [''],
    });
    
  }
  fetch(cb) {
    this.loadingIndicator = true;
    this.credentialService.list().subscribe({ 
      next: (credentials) => {
        this.credentialsData = credentials;
        this.filteredData = credentials;
      }, 
      error: (error) => {
        this.showNotification(
          'bg-red',
          `Credential list load failed ${error}`,
          'bottom',
          'right'
          );
        },
        complete: () =>  { this.loadingIndicator = false;    }
      });
  }
  // Table 2

  editRow(row, rowIndex, content) {
    var credentialRecord: CredentialDatabaseModel;
    this.credentialService.getByID(row.id).subscribe({next: (record) => {
      credentialRecord = record;
      console.log(credentialRecord);
      this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
      this.updateCredentialFG.setValue({
        id: credentialRecord.id,
        credential: credentialRecord.credential,
        username: credentialRecord.username,
        row_version: credentialRecord.row_version,
        password: null
      });
      this.selectedRowData = credentialRecord;
    }});
    

  }
  addRow(content) {
    this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
    this.createCredentialFG.patchValue({
      
    });
  }
  deleteRow(row) {
    this.credentialService.delete(row.id).subscribe({ 
      next: (newRecord) => {
        this.showNotification(
          'bg-green',
          'Deleted Record Successfully',
          'bottom',
          'right'
        );
        this.fetch((data) => {
          this.credentialsData = data;
          this.filteredData = data;
        });
       }, 
       error: (error) => {
        this.showNotification(
          'bg-red',
          `Credential delete failed: ${error}`,
          'bottom',
          'right'
        );
       }});
  }
  arrayRemove(array, id) {
    return array.filter(function (element) {
      return element.id != id;
    });
  }
  onEditSave(form: FormGroup) {
    var updateCredential: CredentialCRUDModel = this.updateCredentialFG.getRawValue();
    this.credentialService.update(updateCredential, this.updateCredentialFG.get("id").value).subscribe({ 
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
          this.credentialsData = data;
          this.filteredData = data;
        });
       }, 
       error: (error) => {
        this.showNotification(
          'bg-red',
          `Credential update failed: ${error}`,
          'bottom',
          'right'
        );
       }});

    this.credentialsData = this.credentialsData.filter((value, key) => {
      if (value.id == form.value.id) {
        value.credential = form.value.credential;
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
  onAddRowSave(form: FormGroup) {
    var newCredential: CredentialCRUDModel = this.createCredentialFG.getRawValue();
    this.credentialService.create(newCredential).subscribe({ 
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
          this.credentialsData = data;
          this.filteredData = data;
        });
       }, 
       error: (error) => {
        this.showNotification(
          'bg-red',
          `Credential create failed ${error}`,
          'bottom',
          'right'
        );
       }});

  }
  filterDatatable(event) {
    // get the value of the key pressed and make it lowercase
    const val = event.target.value.toLowerCase();
    // get the amount of columns in the table
    const colsAmt = this.columns.length;
    // get the key names of each column in the dataset
    const keys = Object.keys(this.filteredData[0]);
    // assign filtered matches to the active datatable
    this.credentialsData = this.filteredData.filter(function (item) {
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

