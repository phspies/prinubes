import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { OrganizationService } from './organization.service';
import { CredentialCRUDModel, CredentialDatabaseModel } from '../models/credentialModels';

@Injectable({
  providedIn: 'root',
})
export class CredentialService {
  private serviceURL: string;
  private contentHeaders = new HttpHeaders();


  constructor(private http: HttpClient, private orgService: OrganizationService) {
  
    this.serviceURL = `${environment.identityApiUrl}/identity/${orgService.getCurrentOrgID()}/credentials`;
  }
  
  list(): Observable<CredentialDatabaseModel[]> {
    return this.http.get<CredentialDatabaseModel[]>(this.serviceURL , { headers: this.contentHeaders} )
      .pipe(
        map((credentialList) => {
          return credentialList;
        }),
          catchError((err) => {
            throw err;
          }));
  }
  update(credential: CredentialCRUDModel, credentialId: string): Observable<CredentialDatabaseModel> {
    return this.http.put<CredentialDatabaseModel>(`${this.serviceURL}/${credentialId}` , JSON.stringify(credential), { headers: this.contentHeaders} )
      .pipe(
        map((crendetialRecord) => {
          return crendetialRecord;
        }),
          catchError((err) => {
            throw err;
          }));
  }
  delete(credentialId: string) {
    return this.http.delete(`${this.serviceURL}/${credentialId}` , { headers: this.contentHeaders} )
      .pipe(catchError((err) => {
            throw err;
          }));
  }
  getByID(credentialId: string) {
    return this.http.get<CredentialDatabaseModel>(`${this.serviceURL}/${credentialId}` , { headers: this.contentHeaders} )
      .pipe(catchError((err) => {
            throw err;
          }));
  }
  create(credential: CredentialCRUDModel): Observable<CredentialDatabaseModel> {
    return this.http.post<CredentialDatabaseModel>(this.serviceURL , JSON.stringify(credential), { headers: this.contentHeaders} )
      .pipe(
        map((crendetialRecord) => {
          return crendetialRecord;
        }),
          catchError((err) => {
            throw err;
          }));
  }
}
