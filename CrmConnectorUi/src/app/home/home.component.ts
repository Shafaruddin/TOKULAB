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
import { ContactModeInputEnum, Customer, DataSetEnum, GetCustomerInputDto, GetCustomerOutputDto, SVCSConnectorServiceProxy } from '@shared/service-proxies/service-proxies';
import { constants } from 'buffer';
import { OtpAuthenticationPopupComponent } from '@app/otp-authentication-popup/otp-authentication-popup.component';
import { AppInsightsService } from '@markpieszak/ng-application-insights';
import { CustomerCaseActivityHistoryComponent } from '@app/customer-case-activity-history/customer-case-activity-history.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class HomeComponent implements OnInit, AfterViewInit {

  private findCustomerModalRef: NgbModalRef = null;
  private caseHistoryAndActivitiesModalRef: NgbModalRef = null;
  private authenticateModalRef: NgbModalRef = null;

  //#region reset when saved
  public caseCustomer: GetCustomerOutputDto;
  public systemAuthenticated: boolean = false;
  public isAnonymousCustomer: boolean = false;
  //#endreion
  public manualAuthenticated: boolean = false;
  public caseUpdated: boolean = false;

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

  ngAfterViewInit(): void {

  }

  findCustomerPopup() {
    this._ai.trackEvent('Find Customer Popup Opened');
    // eval(`$('#customer-search').modal()`);
    this.findCustomerModalRef = this.modalService.open(CustomerSearchPopupComponent, {
      size: 'xl'
    });
    var $scope = this;
    this.findCustomerModalRef.result.then(function (value: Customer) {
      if (typeof (value) == typeof (new GetCustomerOutputDto())) { //object
        $scope.caseCustomer = new GetCustomerOutputDto();
        $scope.caseCustomer.idNumber = value.idNumber;
        $scope.caseCustomer.customerGUID = value.customerGUID;
        $scope.caseCustomer.customerName = value.customerName;
        $scope.caseCustomer.accountNumber = value.accountNo;
        $scope.caseCustomer.emailAddress = value.emailAddress;
        $scope.caseCustomer.mobileNo = value.mobileNo;
        // $scope.systemAuthenticated = false;
        $scope.isAnonymousCustomer = false;
        $scope.systemAuthenticated = false;
        $scope.appConsts.ciscoParameters.systemAuthenticatedNAReason = 'NO';

        $scope._ai.trackEvent('Find Customer Popup Selected Customer', { customerGUID: value.customerGUID });
        $scope.reloadCustomer($scope.caseCustomer, 'WEBFORM: Find Customer PopUp');
        
      }
    }, (reason) => {
        this._ai.trackEvent('Find Customer Popup rejected/dismissed');
    })
  }

  caseHistoryAndActivitiesPopup() {
    this._ai.trackEvent('Case History And Activities Popup Opened');
    this.caseHistoryAndActivitiesModalRef = this.modalService.open(CustomerCaseActivityHistoryComponent, {
      size: 'xl'
    });
    this.caseHistoryAndActivitiesModalRef.componentInstance.customerGuid = this.caseCustomer.customerGUID;
    this.caseHistoryAndActivitiesModalRef.result.then(function (value: Customer) {
      this._ai.trackEvent('Case History And Activities Popup closed');
    }, (reason) => {
      this._ai.trackEvent('Case History And Activities Popup rejected/dismissed');
    })
  }

  ngOnInit(): void {
    if (
      !AppConsts.ciscoParameters.caseId ||
      !AppConsts.ciscoParameters.iPCCCallExtensionID ||
      !AppConsts.ciscoParameters.agentUsername ||
      !AppConsts.ciscoParameters.agentName ||
      !AppConsts.ciscoParameters.contactMode ||
      !AppConsts.ciscoParameters.caseTitle ||
      (!AppConsts.ciscoParameters.customerIsAnonymous && !AppConsts.ciscoParameters.customerId)
    ) {
      abp.notify.error(
        (AppConsts.ciscoParameters.caseId ? '' : 'caseId ') +
        (AppConsts.ciscoParameters.iPCCCallExtensionID ? '' : 'iPCCCallExtensionID ') +
        (AppConsts.ciscoParameters.agentUsername ? '' : 'agentUsername ') +
        (AppConsts.ciscoParameters.agentName ? '' : 'agentName ') +
        (AppConsts.ciscoParameters.contactMode ? '' : `contactMode (${ContactModeInputEnum.Inbound},${ContactModeInputEnum.Outbound}) `) +
        (AppConsts.ciscoParameters.caseTitle ? '' : 'caseTitle ') +
        ((!AppConsts.ciscoParameters.customerIsAuthenticated && !AppConsts.ciscoParameters.systemAuthenticatedNAReason) ? '' : 'systemAuthenticatedNAReason ') +
        ((!AppConsts.ciscoParameters.customerIsAnonymous && !AppConsts.ciscoParameters.customerId) ? 'customerId is needed if not anonymous' : ''),
        'Required Parameters Are Missing'
      );
    } else {
      AppConsts.parametersAreOk = true;
    }

    this._ai.trackEvent('Intialize Parameters',
      {
        caseId: this.appConsts.ciscoParameters.caseId,
        iPCCCallExtensionID: this.appConsts.ciscoParameters.iPCCCallExtensionID,
        customerId: this.appConsts.ciscoParameters.customerId,
        customerIsAnonymous: this.appConsts.ciscoParameters.customerIsAnonymous.toString(),
        customerIsAuthenticated: this.appConsts.ciscoParameters.customerIsAuthenticated.toString(),
        agentUsername: this.appConsts.ciscoParameters.agentUsername,
        agentName: this.appConsts.ciscoParameters.agentName,
        contactMode: this.appConsts.ciscoParameters.contactMode,
        caseTitle: this.appConsts.ciscoParameters.caseTitle,
      }
    );

    if (AppConsts.parametersAreOk) {
      this._ai.trackEvent('Parameters are ok');

      this.systemAuthenticated = this.appConsts.ciscoParameters.customerIsAuthenticated;
      this.isAnonymousCustomer = this.appConsts.ciscoParameters.customerIsAnonymous;

      // this._ai.trackEvent('GetCustomer Complete', { CustomerGUID: this.caseCustomer.customerGUID });

      // load customer or anonymous customer

      if (this.appConsts.ciscoParameters.customerIsAnonymous) {
        this.loadAnonymousCustomer();
      } else {
        var temp = new GetCustomerOutputDto();
        temp.idNumber = null;
        temp.customerGUID = this.appConsts.ciscoParameters.customerId;
        temp.customerName = null;
        temp.accountNumber = null;
        temp.emailAddress = null;
        temp.mobileNo = null;
        this.reloadCustomer(temp, 'WEBFORM: Initialize');
      }
    } else {
      this._ai.trackEvent('Parameters are not ok');
    }

    // eval(`$('#customer-search').modal('hide')`);
    this.modalService.dismissAll();
  }

  reloadCustomer(customerInput: GetCustomerOutputDto, triggeredFrom: string, isAnonymous = false) {
    this._ai.startTrackEvent('GetCustomer');
    this.loading = true;
    this._sVCSConnectorServiceProxy
      .getCustomer(AppConsts.ciscoParameters.apiToken,new GetCustomerInputDto({
        idNumber: null, //customerInput.idNumber?.replace(/\D/g, '').substring(0, 7) ?? null,
        customerGUID: customerInput.customerGUID,
        spaAccountNumber: null, //customerInput.accountNumber,
        anonymous: isAnonymous,
        dataSet: DataSetEnum.FullData,
        // dataSet: isAnonymous ? DataSetEnum.FullData : (this.systemAuthenticated ? DataSetEnum.FullData : DataSetEnum.Subset),
        triggeredFrom: triggeredFrom,
        triggeredBy: AppConsts.ciscoParameters.agentName,
      }))
      .pipe(
        finalize(() => {
          this.loading = false;
          this._ai.stopTrackEvent('GetCustomer');
        })
      )
      .subscribe((result) => {
        this.loading = false;
        this.caseCustomer = result;
        this._ai.trackEvent('GetCustomer Complete', { CustomerGUID: this.caseCustomer.customerGUID });
        if (isAnonymous) {
          this.isAnonymousCustomer = true;
          // this.systemAuthenticated = true;
        }
      });
  }

  public tempAuthenticateTestPhoneNumbers = [10, 11, 12, 13, 14, 15, 18];

  authenticatePopup(mobile: string) {
    // eval(`$('#customer-search').modal()`);
    this.authenticateModalRef = this.modalService.open(OtpAuthenticationPopupComponent);
    this.authenticateModalRef.componentInstance.mobile = mobile.toString();
    var $scope = this;
    this.authenticateModalRef.result.then(function (value) {
      if (typeof (value) == typeof (true)) { //bool
        $scope.systemAuthenticated = true;
      }
    }, (reason) => {
      $scope.systemAuthenticated = false;
    })
  }

  loadAnonymousCustomer() {
    var anonymouRequest = new GetCustomerOutputDto();
    anonymouRequest.idNumber = null;
    anonymouRequest.customerGUID = null;
    anonymouRequest.accountNumber = null;
    this.reloadCustomer(anonymouRequest, 'WEBFORM: Initialize', true);
  }

  getSystemAuthenticatedStatus() {
    var authenticated = this.systemAuthenticated;
    var authenticatedText = 'System Authenticated';
    if(this.isAnonymousCustomer){
      authenticated = false;
      authenticatedText = `System Authenticated: ${this.appConsts.ciscoParameters.systemAuthenticatedNAReason}`;
    }else if (this.caseCustomer) {
      if (this.caseCustomer?.accountStatus?.toUpperCase() === 'ACTIVE') {
        authenticatedText = this.systemAuthenticated ? 'System Authenticated: YES' : `System Authenticated: ${this.appConsts.ciscoParameters.systemAuthenticatedNAReason}`
      } else {
        authenticated = false;
        authenticatedText = `System Authenticated: ${this.appConsts.ciscoParameters.systemAuthenticatedNAReason}`;
      }
    }
    return {
      authenticated: authenticated,
      text: authenticatedText
    }
  }

  manualAuthenticatedFieldChanged($event: boolean) {
    this.manualAuthenticated = $event;
  }

  caseUpdatedEventHandler($event: boolean) {
    this.caseUpdated = $event;
  }

}
