import { Component, EventEmitter, Input, OnInit, Output, ViewEncapsulation } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { AppConsts } from '@shared/AppConsts';
import { CaseStatusInputEnum, Category, ContactModeInputEnum, CreateCaseInputDto, Customer, FindCustomerInputDto, FindCustomerOutputDto, GetCategoryAndCriteriaOutputDto, GetCustomerOutputDto, SVCSConnectorServiceProxy, UpdateCaseInputDto } from '@shared/service-proxies/service-proxies';
import { debug } from 'console';
import { abort } from 'process';
import { finalize } from 'rxjs/operators';
import { AppInsightsService } from '@markpieszak/ng-application-insights';
import { ActivatedRoute } from '@angular/router';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-create-case-form',
  templateUrl: './create-case-form.component.html',
  styleUrls: ['./create-case-form.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class CreateCaseFormComponent implements OnInit {

  @Input() public customer: GetCustomerOutputDto | null;
  @Input() public isAnonymous: boolean | false;
  @Input() public caseId: string | undefined;
  @Output() caseCreatedEvent = new EventEmitter<boolean>();
  consts = AppConsts;

  screenName = 'WEBFORM: Create Case Form';

  loading = false;

  input: CreateCaseInputDto = new CreateCaseInputDto();

  createCaseForm = new FormGroup({
    caseCategory1: new FormControl('', [Validators.required]),
    caseCategory2: new FormControl('', [Validators.required]),
    caseCategory3: new FormControl('', [Validators.required]),
    caseTitle: new FormControl(``, [Validators.required, Validators.maxLength(300)]),
    systemAuthenticated: new FormControl(false),
    callBack: new FormControl(false),
    caseDetails: new FormControl('', [Validators.required, Validators.maxLength(10000)]),

    ipccCallExtensionID: new FormControl(``, [Validators.required, Validators.maxLength(36), Validators.pattern('^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$')]),
    primaryCaseOfficer: new FormControl(``, [Validators.required, Validators.maxLength(100)]),
    contactMode: new FormControl(ContactModeInputEnum.Inbound, [Validators.required]),

    // agent's details
    createdBy: new FormControl(this.input.createdBy, [Validators.required, Validators.maxLength(100)]),
    triggeredBy: new FormControl(this.input.triggeredBy, [Validators.required, Validators.maxLength(100)]),
  });

  public dropdownData: GetCategoryAndCriteriaOutputDto = new GetCategoryAndCriteriaOutputDto({
    adhocCriteria: [],
    categories: [],
    returnStatus: null
  });

  public contactModes = [
    {
      name: "Inbound",
      value: ContactModeInputEnum.Inbound
    },
    {
      name: "Outbound",
      value: ContactModeInputEnum.Outbound
    }];

  public cat1Options: Category[] = [];
  public cat2Options: Category[] = [];
  public cat3Options: Category[] = [];

  constructor(
    private _sVCSConnectorServiceProxy: SVCSConnectorServiceProxy,
    private _ai: AppInsightsService,
    private route: ActivatedRoute,
  ) { }

  ngOnInit(): void {
    this.testNav();
    this.input.createdBy = AppConsts.ciscoParameters.agentUsername || "";
    this.input.triggeredBy = AppConsts.ciscoParameters.agentName || "";
    this.input.primaryCaseOfficer = AppConsts.ciscoParameters.agentUsername || "";
    this.input.ipccCallExtensionID = AppConsts.ciscoParameters.iPCCCallExtensionID || "";

    this.createCaseForm.controls.createdBy.setValue(this.input.createdBy);
    this.createCaseForm.controls.triggeredBy.setValue(this.input.triggeredBy);
    this.createCaseForm.controls.primaryCaseOfficer.setValue(this.input.primaryCaseOfficer);
    this.createCaseForm.controls.ipccCallExtensionID.setValue(this.input.ipccCallExtensionID);

    this._ai.startTrackEvent('Update Case Form - Prepare Dropdowns');
    this.createCaseForm.controls.caseCategory1.disable();
    this.createCaseForm.controls.caseCategory2.disable();
    this.createCaseForm.controls.caseCategory3.disable();
    this.input.contactMode = ContactModeInputEnum.Outbound;
    this.input.caseTitle = `${AppConsts.ciscoParameters.caseTitle}`;
    this.input.systemAuthenticated = false;
    this.loading = true;
    this._sVCSConnectorServiceProxy.getCategoryAndCriteria(AppConsts.ciscoParameters.apiToken)
      .pipe(
        finalize(() => {
          this._ai.stopTrackEvent('Update Case Form - Prepare Dropdowns');
          this.loading = false;
        })
      )
      .subscribe((result) => {
        this.loading = false;
        this.dropdownData = result;
        this.cat1Options = this.dropdownData.categories.filter(x => x.type === AppConsts.categorySelect.Category1);
        this.cat2Options = this.dropdownData.categories.filter(x => x.type === AppConsts.categorySelect.Category2);
        this.cat3Options = this.dropdownData.categories.filter(x => x.type === AppConsts.categorySelect.Category3);
        this.createCaseForm.controls.caseCategory1.enable();
        this.createCaseForm.controls.caseCategory2.enable();
        this.createCaseForm.controls.caseCategory3.enable();
      });
  }

  createCase() {

    this.input.customerGUID = this.customer?.customerGUID ?? AppConsts.ciscoParameters.customerId;
    if (!this.input.customerGUID) {
      abp.message.error('Please Assign a customer', 'Missing Customer');
      return;
    }

    this.input.triggeredFrom = this.screenName;
    // this.input.triggeredBy = AppConsts.ciscoParameters.agentName; //model
    // this.input.createdBy = AppConsts.ciscoParameters.agentUsername; //model
    this.input.modifiedBy = this.input.createdBy;
    this.input.owner = this.input.createdBy;
    this.input.primaryCaseOfficer = this.input.createdBy;
    this.input.dateTimeReceived = DateTime.now();
    this.input.createdOn = DateTime.now();

    // "customerGUID": "3fa85f64-5717-4562-b3fc-2c963f66afa6", //done
    // "caseTitle": "string", //view,model
    // "caseDetails": "string", //view,model
    // "systemAuthenticated": true, //view,model
    // "callBack": view,model,
    // "caseCategory1": "3fa85f64-5717-4562-b3fc-2c963f66afa6", //view,model
    // "caseCategory2": "3fa85f64-5717-4562-b3fc-2c963f66afa6", //view,model
    // "caseCategory3": "3fa85f64-5717-4562-b3fc-2c963f66afa6", //view,model
    // "triggeredFrom": "string", //model
    // "triggeredBy": "string" //view,model
    // "createdBy": "string", //view,model
    // "modifiedBy": "string", //model
    // "owner": "string", //model
    // "dateTimeReceived": "2022-08-06T13:56:51.074Z", //model,luxon
    // "createdOn": "2022-08-06T13:56:51.074Z", //model,luxon
    // "contactMode": "Inbound", //vide

    // ToDo

    // "ipccCallExtensionID": "string",
    // "primaryCaseOfficer": "string",

    this._ai.trackEvent('CreateCase - Data', { data: this.input.toJSON() });
    this._ai.startTrackEvent('CreateCase');
    this.loading = true;
    this._sVCSConnectorServiceProxy
      .createCase(AppConsts.ciscoParameters.apiToken,this.input)
      .pipe(
        finalize(() => {
          this._ai.stopTrackEvent('CreateCase');
          this.loading = false;
        })
      )
      .subscribe((result) => {
        abp.message.success(result.caseID, "Case Created");
        AppConsts.ciscoParameters.caseId = result.caseID;
        AppConsts.ciscoParameters.caseTitle = this.input.caseTitle;

        AppConsts.ciscoParameters.caseId = result.caseID;
        AppConsts.ciscoParameters.iPCCCallExtensionID = this.input.ipccCallExtensionID;
        AppConsts.ciscoParameters.customerId = this.input.customerGUID;
        AppConsts.ciscoParameters.customerIsAnonymous = this.isAnonymous;
        AppConsts.ciscoParameters.customerIsAuthenticated = this.input.systemAuthenticated;
        AppConsts.ciscoParameters.systemAuthenticatedNAReason = "Manual";
        AppConsts.ciscoParameters.agentUsername = this.input.createdBy;
        AppConsts.ciscoParameters.agentName = this.input.triggeredBy;
        AppConsts.ciscoParameters.contactMode = this.input.contactMode;
        AppConsts.ciscoParameters.caseTitle = this.input.caseTitle;
        this.disableForm();
      });
  }

  disableForm() {
    this.caseCreatedEvent.emit(true);
    this.createCaseForm.disable();
  }

  onSubmit() {
    return false;
    this.loading = true;

    // var inputData = new FindCustomerInputDto({
    //   name: (this.findCustomerForm.controls.name.value),
    //   idNumber: (this.findCustomerForm.controls.identificatioNo.value),
    //   accountNumber: (this.findCustomerForm.controls.accountNo.value),
    //   mobileNo: (this.findCustomerForm.controls.mobile.value),
    //   emailAddress: (this.findCustomerForm.controls.email.value),
    //   triggeredFrom: this.screenName,
    //   triggeredBy: AppConsts.ciscoParameters.agentName
    // });

    // this._sVCSConnectorServiceProxy
    //   .getCategoryAndCriteria()
    //   .pipe(
    //     finalize(() => {
    //       this.loading = false;
    //     })
    //   )
    //   .subscribe((result) => {
    //     this.loading = false;
    //     this.dropdownData = result;
    //   });
  }

  categoriesOnClear(caseCategoryNumber: number) {
    switch (caseCategoryNumber) {
      case 1:
        this.input.caseCategory1 = '';
        this.createCaseForm.controls.caseCategory1.setValue(null);
        this.cat2Options = this.dropdownData.categories.filter(x => x.type === AppConsts.categorySelect.Category2);
      case 2:
        this.input.caseCategory2 = '';
        this.createCaseForm.controls.caseCategory2.setValue(null);
        this.cat3Options = this.dropdownData.categories.filter(x => x.type === AppConsts.categorySelect.Category3);
      // this.createCaseForm.controls.caseCategory3.disable();
      case 3:
        this.input.caseCategory3 = '';
        this.createCaseForm.controls.caseCategory3.setValue(null);
      default:
        break;
    }
  }

  categoriesOnChange($event: Category, caseCategoryNumber: number) {
    if (!$event) {
      return;
    }
    switch (caseCategoryNumber) {
      case 1:
        this.cat2Options = this.dropdownData.categories.filter(x => x.relatedCategory1ID == $event.categoryId && x.type === AppConsts.categorySelect.Category2)
        this.input.caseCategory2 = '';
        this.createCaseForm.controls.caseCategory2.setValue(null);

        this.createCaseForm.controls.caseCategory2.enable();
        this.createCaseForm.controls.caseCategory3.disable();
      case 2:
        this.cat3Options = this.dropdownData.categories.filter(x => x.relatedCategory2ID == $event.categoryId && x.type === AppConsts.categorySelect.Category3)
        this.input.caseCategory3 = '';
        this.createCaseForm.controls.caseCategory3.setValue(null);

        this.createCaseForm.controls.caseCategory3.enable();

        if (!this.input.caseCategory1) {
          this.input.caseCategory1 = $event.relatedCategory1ID;
          this.createCaseForm.controls.caseCategory1.setValue(this.input.caseCategory1);
        }

        break;

      case 3:
        this.cat2Options = this.dropdownData.categories.filter(x => x.relatedCategory1ID == $event.relatedCategory1ID && x.type === AppConsts.categorySelect.Category2)
        this.input.caseCategory1 = $event.relatedCategory1ID;
        this.input.caseCategory2 = $event.relatedCategory2ID;
        this.createCaseForm.controls.caseCategory1.setValue(this.input.caseCategory1);
        this.createCaseForm.controls.caseCategory2.setValue(this.input.caseCategory2);
        this.createCaseForm.controls.caseCategory1.enable();
        this.createCaseForm.controls.caseCategory2.enable();
        break;
      default:
        break;
    }
  }

  testNav(){
    return;
    AppConsts.ciscoParameters.caseId = "C123";
    AppConsts.ciscoParameters.iPCCCallExtensionID = "IPCC123";
    AppConsts.ciscoParameters.customerId = "GUID123";
    AppConsts.ciscoParameters.customerIsAnonymous = true;
    AppConsts.ciscoParameters.customerIsAuthenticated = false;
    AppConsts.ciscoParameters.systemAuthenticatedNAReason = "Manual";
    AppConsts.ciscoParameters.agentUsername = "createdBy123";
    AppConsts.ciscoParameters.agentName = "triggeredBy123";
    AppConsts.ciscoParameters.contactMode = ContactModeInputEnum.Inbound;
    AppConsts.ciscoParameters.caseTitle = "caseTitle123";
  }
}