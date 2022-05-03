import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import { User } from '../models/user';
import { environment } from 'src/environments/environment';
import {userAuthenticate} from "../models/userAuthenticate";
import { OrganizationService } from './organization.service';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private currentUserSubject: BehaviorSubject<User>;
  public currentUser: Observable<User>;
  private serviceURL = `${environment.identityApiUrl}/identity/users`;
  private authticateURL = `${this.serviceURL}/authenticate`;
  private contentHeaders = new HttpHeaders();


  constructor(private http: HttpClient, private organizationService: OrganizationService) {
    this.contentHeaders.set('content-type', 'application/json');
    this.currentUserSubject = new BehaviorSubject<User>(
      JSON.parse(localStorage.getItem('currentUser'))
    );
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User {
    return this.currentUserSubject.value;
  }

  login(username: string, password: string): Observable<any> {
    const body: userAuthenticate = { username, password };
    return this.http.post<any>(this.authticateURL, body , { headers: this.contentHeaders} )
      .pipe(
        map((user) => {
          this.currentUserSubject.next(user);
          localStorage.setItem('currentUser', JSON.stringify(user));
          this.organizationService.getCurrent().subscribe(org => { 
            console.debug(org);
          });
          return user; 

          
        }),
          catchError((err) => {
            console.error(err);
            throw err;
          }));
  }

  logout() {
    // remove user from local storage to log user out
    localStorage.removeItem('currentUser');
    localStorage.removeItem('currentOrganization');
    localStorage.removeItem('currentOrganizations'); 
    localStorage.removeItem('availableOrganizations'); 
    this.currentUserSubject.next(null);
    return of({ success: false });
  }
}
