import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { OrganizationDatabaseModel } from '../models/organizationDatabaseModel';

@Injectable({
  providedIn: 'root',
})
export class OrganizationService {
  public currentOrganization: Observable<OrganizationDatabaseModel>;
  private currentOrganizationSubject: BehaviorSubject<OrganizationDatabaseModel>;

  private serviceURL = `${environment.identityApiUrl}/identity/organizations`;
  private contentHeaders = new HttpHeaders();
  private organizationList: OrganizationDatabaseModel[]; 

  constructor(private http: HttpClient) {
    this.contentHeaders.append('Content-Type', 'application/json');

    this.currentOrganizationSubject = new BehaviorSubject<OrganizationDatabaseModel>(
      JSON.parse(localStorage.getItem('currentOrganization'))
    );
    this.currentOrganization = this.currentOrganizationSubject.asObservable();
  }

  getCurrent(): Observable<any> {
    if (localStorage.getItem('currentOrganization') === null)
    {
      this.list().subscribe(orgs => { 
        this.organizationList = orgs as OrganizationDatabaseModel[];
        localStorage.setItem('currentOrganization', JSON.stringify(this.organizationList[0]));
      });
    }
    else
    {
      var org = JSON.parse(localStorage.getItem('currentOrganization')) as OrganizationDatabaseModel;
      console.log(org);
      return this.http.get<any>(`${this.serviceURL}/${org.id}` , { headers: this.contentHeaders} )
    }
  }
  getCurrentOrgID(): string {
    if (localStorage.getItem('currentOrganization') === null)
    {
      this.list().subscribe(orgs => { 
        this.organizationList = orgs as OrganizationDatabaseModel[];
        localStorage.setItem('currentOrganization', JSON.stringify(this.organizationList[0]));
      });
      
    }
    var org = JSON.parse(localStorage.getItem('currentOrganization')) as OrganizationDatabaseModel;
    return org.id;
  }
  list(): Observable<any> {
    return this.http.get<any>(this.serviceURL , { headers: this.contentHeaders} )
      .pipe(
        map((organizations) => {
          localStorage.setItem('availableOrganizations', JSON.stringify(organizations));
          this.currentOrganizationSubject.next(organizations[0]);
          return organizations;
        }),
          catchError((err) => {
            console.error(err);
            throw err;
          }));
  }
}
