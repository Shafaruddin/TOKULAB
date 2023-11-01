import { Component, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppConsts } from '@shared/AppConsts';
import { GetCaseAndActivitiesHistoryInputDto, GetCaseAndActivitiesHistoryOutputDto, SVCSConnectorServiceProxy } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';
import { AppInsightsService } from '@markpieszak/ng-application-insights';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-customer-case-activity-history',
  templateUrl: './customer-case-activity-history.component.html',
  styleUrls: ['./customer-case-activity-history.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class CustomerCaseActivityHistoryComponent implements OnInit {

  loading = false;
  @Input() public customerGuid: string | null;
  screenName = 'WEBFORM: Activity Screen';

  data: GetCaseAndActivitiesHistoryOutputDto = new GetCaseAndActivitiesHistoryOutputDto({
    recentCaseHistory: [],
    recentCallActivities: [],
    recentSMSActivities: [],
    recentEmailActivities: [],
    returnStatus: null
  });

  constructor(
    public activeModal: NgbActiveModal,
    private route: ActivatedRoute,
    private _sVCSConnectorServiceProxy: SVCSConnectorServiceProxy,
    private _ai: AppInsightsService,
  ) { }

  dismiss(data: any) {
    this.activeModal.dismiss(data)
  }

  ngOnInit(): void {

    this.route.queryParams.subscribe(params => {
      if (Object.keys(params).length === 0) {
        return;
      }

      AppConsts.ciscoParameters.agentName = params.agentName || "";
      // AppConsts.ciscoParameters.apiToken = `Basic ${params.authToken || ""}`;

      if (
        !AppConsts.ciscoParameters.agentName
      ) {
        abp.notify.error(
          (AppConsts.ciscoParameters.agentName ? '' : 'agentName '),
          'Required Parameters Are Missing'
        );
      } else {
        AppConsts.parametersAreOk = true;
      }
    })

    this._ai.trackEvent('Customer History', { customerGuid: this.customerGuid });
    this.reload();
  }

  reload() {
    this.loading = true;

    var inputData = new GetCaseAndActivitiesHistoryInputDto({
      customerGuid: this.customerGuid,
      triggeredFrom: this.screenName,
      triggeredBy: AppConsts.ciscoParameters.agentName
    });
    this._ai.startTrackEvent('GetCaseAndActivitiesHistory');
    this._sVCSConnectorServiceProxy
      .getCaseAndActivitiesHistory(AppConsts.ciscoParameters.apiToken,inputData)
      .pipe(
        finalize(() => {
          this._ai.stopTrackEvent('GetCaseAndActivitiesHistory');
          this.loading = false;
        })
      )
      .subscribe((result) => {
        this.loading = false;
        this.data = result;
      });
  }
}
