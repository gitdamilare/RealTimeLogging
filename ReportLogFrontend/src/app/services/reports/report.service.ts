import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';

import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  signalrUpdate$ = new BehaviorSubject<any>(null);

  signalrLogData$ = new BehaviorSubject<any>(null);

  constructor(private httpClient: HttpClient) {
      this.connectToSignalR();
   }


  getReportSummary(): Observable<{}> {
    return this.httpClient.get(`${environment.baseApiUrl}/Report`);
  }


  getAllLogs(pageIndex: number, pageSize: number): Observable<{}> {
    return this.httpClient.get(`${environment.baseApiUrl}/Report/${pageIndex}/${pageSize}`);
  }

  /* getAllDataLogs(): Observable<{}> {
    return this.httpClient.post<any>("http://localhost:7071/api/getReportData", {"data" : 1});
  } */


  connectToSignalR() {
    let connection = new signalR.HubConnectionBuilder()
        .withUrl(environment.baseSignalRUrl)
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("newUpdate", data => {
      console.log(data);
        this.signalrUpdate$.next(data);
    });

    //connection.on("getreadyReportData", data => {
    //  console.log(data);
   //     this.signalrLogData$.next(data);
///});

    connection.start()
    // .then(() => connection.invoke("newUpdate", "Hello"));
  }
}
