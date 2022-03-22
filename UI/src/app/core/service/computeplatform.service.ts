import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { OrganizationService } from './organization.service';
import { ComputePlatformCRUDModel, ComputePlatformDatabaseModel } from '../models/computeplatformModel';

@Injectable({
  providedIn: 'root',
})
export class ComputePlatformService {
  private serviceURL: string;

  constructor(private http: HttpClient, private orgService: OrganizationService) {
    this.serviceURL = `${environment.platformApiUrl}/platform/${orgService.getCurrentOrgID()}/computeplatforms`;
  }
  
  list(): Observable<any> {
    return this.http.get<any>(this.serviceURL)
      .pipe(
        map((ComputePlatform) => {
          return ComputePlatform;
        }),
          catchError((err) => {
            throw err;
          }));
  }
  update(platform: ComputePlatformCRUDModel, platformId: string): Observable<any> {
    return this.http.put<any>(`${this.serviceURL}/${platformId}` , JSON.stringify(platform))
      .pipe(
        map((ComputePlatform) => {
          return ComputePlatform;
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
    return this.http.get<ComputePlatformCRUDModel>(`${this.serviceURL}/${platformId}`)
      .pipe(catchError((err) => {
            throw err;
          }));
  }
  create(platform: ComputePlatformCRUDModel): Observable<any> {
    return this.http.post<ComputePlatformDatabaseModel>(this.serviceURL , JSON.stringify(platform))
      .pipe(
        map((ComputePlatform) => {
          return ComputePlatform;
        }),
          catchError((err) => {
            throw err;
          }));
  }
}
