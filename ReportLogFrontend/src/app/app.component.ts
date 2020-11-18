import { Component } from '@angular/core';

import { NzMessageService } from 'ng-zorro-antd/message';

import { ReportService } from './services/reports/report.service';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'dashboard';
  reportSummary: { totalMessage?: number, totalWarning?: number , totalInfo?: number, totalError?: number, totalLogFile?: number, } = {};
  logsDataSet: { data?: any, processing?: boolean, total? : number } = { data: []};
  filesDataSet: { data?: any, processing?: boolean } = { data: []};
  showUpdateAlert: boolean = false;
  showIncomingUpdateAlert: boolean = false;
  fileUploadModalIsVisible: boolean = false;
  baseApiUrl: string = environment.baseApiUrl;
  fileUploadResponse: { message?: string, status?: string} = {};
  pageIndex = 1;
  pageSize = 10;
  total = 1;
  sortValue: string | null = null;
  sortKey: string | null = null;

  constructor(private reportService: ReportService, private message: NzMessageService) { }

  ngOnInit(): void {
    this.loadPageData();
    this.reportService.signalrUpdate$.subscribe((data) => {
      if(data) {
        //this.showUpdateAlert = true;
        this.showIncomingUpdateAlert = false;
        this.reportSummary = data;
        this.filesDataSet = { data: data?.logProcessOutputDto, processing: false } ;
      }
    })
  }

  loadPageData() {
    this.getReportSummary();
    this.getReport();
  }

  reloadPageData() {
    //this.showUpdateAlert = false
    this.showIncomingUpdateAlert = false;
    this.loadPageData();
  }

  getReportSummary() {
    this.filesDataSet.processing = true;
    this.reportService.getReportSummary().subscribe((data: any) => {
      if (data != null && !data.error) {
        this.reportSummary = data.data;
        this.filesDataSet = { data: data?.data?.logProcessOutputDto, processing: false } ;
      }
    }, (err) => console.log(err));
  }

  getReport() {
    this.logsDataSet.processing = true;
    this.reportService.getAllLogs(this.pageIndex, this.pageSize).subscribe((data: any) => {
      if (data != null && !data.error) {
        console.log(data?.data?.results);
        this.total = data?.data?.rowCount;
        this.logsDataSet = { data: data?.data?.results, processing: false, total : this.total} ;
      }
    }, (err) => console.log(err));
  }

  showUpdate() {
    this.showUpdateAlert = true;
  }

  toggleFileUploadModal() {
    this.fileUploadModalIsVisible = !this.fileUploadModalIsVisible;
  }

  uploadFiles(data: any) {
    console.log(data);
    if (data?.type === 'success' || data?.type === 'done') {
      this.message.success(`file uploaded successfully.`);
      this.toggleFileUploadModal()
      this.showIncomingUpdateAlert = true;
    } else if (data?.type === 'error') {
      this.message.error(data?.file?.error?.error?.data?.length ? data?.file?.error.error?.data[0] : `file upload failed.`);
    }
  }

  changePageIndex(pageIndex : any ) {
    this.pageIndex = pageIndex;
    this.getReport();
  }
   changePageSize(pageSize : any) {
    this.pageSize = pageSize;
     this.getReport();
  }

}
