import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { OrganizationService } from './organization.service';
import { LoadBalancerPlatformCRUDModel, LoadBalancerPlatformDatabaseModel } from '../models/loadbalancerplatformModel';

@Injectable({
  providedIn: 'root',
})
export class LoadBalancerPlatformService {
  private serviceURL: string;

  constructor(private http: HttpClient, private orgService: OrganizationService) {
    this.serviceURL = `${environment.platformApiUrl}/platform/${orgService.getCurrentOrgID()}/loadbalancerplatforms`;
  }
  
  list(): Observable<any> {
    return this.http.get<any>(this.serviceURL)
      .pipe(
        map((LoadBalancerPlatform) => {
          return LoadBalancerPlatform;
        }),
          catchError((err) => {
            throw err;
          }));
  }
  update(platform: LoadBalancerPlatformCRUDModel, platformId: string): Observable<any> {
    return this.http.put<any>(`${this.serviceURL}/${platformId}` , JSON.stringify(platform))
      .pipe(
        map((LoadBalancerPlatform) => {
          return LoadBalancerPlatform;
        }),
          catchError((err) => {
            throw err;
          }));
  }
  delete(platformId: string): Observable<any> {
    return this.http.delete<any>(`${this.serviceURL}/${platformId}`)
      .pipe(catchError((err) => {
            throw err;
          }));
  }
  getByID(platformId: string) {
    return this.http.get<LoadBalancerPlatformCRUDModel>(`${this.serviceURL}/${platformId}`)
      .pipe(catchError((err) => {
            throw err;
          }));
  }
  create(platform: LoadBalancerPlatformCRUDModel): Observable<any> {
    return this.http.post<LoadBalancerPlatformDatabaseModel>(this.serviceURL , JSON.stringify(platform))
      .pipe(
        map((LoadBalancerPlatform) => {
          return LoadBalancerPlatform;
        }),
          catchError((err) => {
            throw err;
          }));
  }
}
