import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppInsightsService } from '@markpieszak/ng-application-insights';
import { AdminServiceProxy } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-retailers',
  templateUrl: './retailers.component.html',
  styleUrls: ['./retailers.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class RetailersComponent implements OnInit {

  adminToken: string;
  public loading = false;

  constructor(
    private route: ActivatedRoute,
    private _ai: AppInsightsService,
    private _adminServiceProxy: AdminServiceProxy
  ) { }

  ngOnInit(): void {
    this.adminToken = this.route.parent.snapshot.paramMap.get('AdminToken') || '';
  }

  excelFileSelected(event) {
    const file: File = event.target.files[0];
    var $this = this;

    abp.message.confirm(
      `Are you sure you want to upload this file? This will remove all existing retailers and add the items in the excel file`,
      'Confirm Overwrite of Existing Retailers',
      function (isConfirmed) {
        if (isConfirmed) {
          $this.loading = true;
          $this._ai.startTrackEvent(`uploadRetailersExcelFile`);

          $this._adminServiceProxy.uploadRetailersExcelFile($this.adminToken, {
            data: file,
            fileName: file.name
          })
            .pipe(
              finalize(() => { $this.loading = false; $this._ai.stopTrackEvent(`uploadRetailersExcelFile`); })
            )
            .subscribe(() => {
              abp.notify.success(`File Uploaded!`);
            });
        }
      }
    );
  }
}
