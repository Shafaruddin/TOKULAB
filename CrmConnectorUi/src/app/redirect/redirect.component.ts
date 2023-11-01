import { AfterViewInit, Component, ElementRef, Inject, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { DOCUMENT } from '@angular/common';
import * as signalR from '@aspnet/signalr';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { CustomerAssignedEvent, GetCustomerByConverationIdOutputDto } from '../customer-assigned-event';
import { AgentDetails } from '../dto/agent-info';
import { NotificationService } from '../services/notification.service';
import { AppConsts } from '@shared/AppConsts';
import { NgbModal, ModalDismissReasons, NgbActiveModal, NgbModalRef, NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';
import { CustomerSearchPopupComponent } from '@app/customer-search-popup/customer-search-popup.component';
import { ContactModeInputEnum, Customer, DataSetEnum, GetCustomerInputDto, GetCustomerOutputDto, GetShortenedUrlInputDto, SVCSConnectorServiceProxy } from '@shared/service-proxies/service-proxies';
import { constants } from 'buffer';
import { OtpAuthenticationPopupComponent } from '@app/otp-authentication-popup/otp-authentication-popup.component';
import { AppInsightsService } from '@markpieszak/ng-application-insights';
import { CustomerCaseActivityHistoryComponent } from '@app/customer-case-activity-history/customer-case-activity-history.component';

@Component({
  selector: 'app-redirect',
  templateUrl: './redirect.component.html',
  styleUrls: ['./redirect.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class RedirectComponent implements OnInit {

  public loading = false;
  public appConsts = AppConsts;

  public constructor(
    private titleService: Title,
    private route: ActivatedRoute,
    private router: Router,
    private notification: NotificationService,
    private http: HttpClient,
    @Inject(DOCUMENT) private document: Document,
    private modalService: NgbModal,
    private _sVCSConnectorServiceProxy: SVCSConnectorServiceProxy,
    private _ai: AppInsightsService,
  ) {

  }

  ngOnInit(): void {

    this.route.paramMap.subscribe(params => {
      const id = params.get('id');

      if (id) {
        this._ai.trackEvent('Load Url',
          {
            Id: id,
          }
        );
        this.loadUrl(id);
      } else {
        this.notification.errorMessage("No Url");
      }
      this.modalService.dismissAll();
    })
  }

  loadUrl(urlId: string) {
    this._ai.startTrackEvent('LoadUrl');
    this.loading = true;
    this._sVCSConnectorServiceProxy
      .getShortenUrl(AppConsts.ciscoParameters.apiToken, new GetShortenedUrlInputDto({
        shortenedUrl: urlId
      }))
      .pipe(
        finalize(() => {
          this.loading = false;
          this._ai.stopTrackEvent('getShortenUrl');
        })
      )
      .subscribe((result) => {
        this.loading = false;
        this._ai.trackEvent('getShortenUrl Complete', { urlId });
        AppConsts.ciscoParameters.caseId = result.caseId || "";
        AppConsts.ciscoParameters.iPCCCallExtensionID = result.ipccCallExtensionID || "";
        AppConsts.ciscoParameters.customerId = result.customerId || "";
        AppConsts.ciscoParameters.customerIsAnonymous = result.customerIsAnonymous == 'true';
        AppConsts.ciscoParameters.customerIsAuthenticated = result.customerIsAuthenticated == 'true';
        AppConsts.ciscoParameters.systemAuthenticatedNAReason = result.systemAuthenticatedNAReason || "";
        AppConsts.ciscoParameters.agentUsername = result.agentUsername || "";
        AppConsts.ciscoParameters.agentName = result.agentName || "";
        AppConsts.ciscoParameters.contactMode = result.contactMode == "Inbound" ? ContactModeInputEnum.Inbound : ContactModeInputEnum.Outbound;
        AppConsts.ciscoParameters.caseTitle = result.caseTitle || "";
        AppConsts.ciscoParameters.apiToken = `Basic ${result.authToken || ""}`;

        this.router.navigate(['/update-case']);
      },
        (error) => {
          this.notification.errorMessage("Error loading Url");
        });
  }
}
