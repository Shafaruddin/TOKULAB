import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewEncapsulation } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { AppConsts } from '@shared/AppConsts';
import { AutoSaveCaseServiceProxy, CaseStatusInputEnum, Category, ContactModeInputEnum, GetCategoryAndCriteriaOutputDto, GetCustomerOutputDto, SVCSConnectorServiceProxy, UpdateCaseAutoSaveInputDto, UpdateCaseInputDto } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';
import { AppInsightsService } from '@markpieszak/ng-application-insights';
import { interval, Subscription } from 'rxjs';
import { KeyValueChanges, KeyValueDiffer, KeyValueDiffers } from '@angular/core';
import { ngDebounce } from 'utils/debounce-decorator';
import { debounce_or_throttle } from 'utils/debounce_or_throttle';

@Component({
  selector: 'app-update-case',
  templateUrl: './update-case.component.html',
  styleUrls: ['./update-case.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class UpdateCaseComponent implements OnInit, OnDestroy {

  input: UpdateCaseInputDto = new UpdateCaseInputDto();

  checkCustomerGuidSubscription: Subscription;
  private updateCaseInputDtoDiffer: KeyValueDiffer<string, any>;

  @Input() public customer: GetCustomerOutputDto | null;
  @Input() public caseId: string | undefined;
  @Input()
  set manualAuthenticated(value: boolean) {
    if (event) {
      this.input.manualVerification = value;
    }
  }
  @Output() caseUpdatedEvent = new EventEmitter<boolean>();

  screenName = 'WEBFORM: Update Case Form';

  callTypes = [ContactModeInputEnum.Inbound, ContactModeInputEnum.Outbound]

  loading = false;
  autoSaveInProgress = false;

  updateCaseForm = new FormGroup({
    caseCategory1: new FormControl('', [Validators.required]),
    caseCategory2: new FormControl('', [Validators.required]),
    caseCategory3: new FormControl('', [Validators.required]),
    caseTitle: new FormControl(``, [Validators.required, Validators.maxLength(300)]),
    adHocCriteria: new FormControl(''),
    caseDetails: new FormControl('', [Validators.required, Validators.maxLength(10000)]),
    followupAction: new FormControl('', [Validators.maxLength(20000)]), // removed Validators.required because if resolved agent won't input it,
  });

  public dropdownData: GetCategoryAndCriteriaOutputDto = new GetCategoryAndCriteriaOutputDto({
    adhocCriteria: [],
    categories: [],
    returnStatus: null
  });

  public cat1Options: Category[] = [];
  public cat2Options: Category[] = [];
  public cat3Options: Category[] = [];

  constructor(
    private _sVCSConnectorServiceProxy: SVCSConnectorServiceProxy,
    private _autoSaveCaseServiceProxy: AutoSaveCaseServiceProxy,
    private _ai: AppInsightsService,
    private differs: KeyValueDiffers
  ) { }

  ngOnInit(): void {
    this._ai.startTrackEvent('Update Case Form - Prepare Dropdowns');
    this.updateCaseForm.controls.caseCategory1.disable();
    this.updateCaseForm.controls.caseCategory2.disable();
    this.updateCaseForm.controls.caseCategory3.disable();
    this.input.contactMode = [ContactModeInputEnum.Inbound, ContactModeInputEnum.Outbound].includes(AppConsts.ciscoParameters.contactMode) ? AppConsts.ciscoParameters.contactMode : ContactModeInputEnum.Inbound;
    this.input.caseTitle = `${AppConsts.ciscoParameters.caseTitle}`;
    this.input.manualVerification = false;
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
        this.updateCaseForm.controls.caseCategory1.enable();
        this.updateCaseForm.controls.caseCategory2.enable();
        this.updateCaseForm.controls.caseCategory3.enable();
      });

    // check for customer assignments every second
    this.checkCustomerGuidSubscription = interval(1000).subscribe(val => {
      this.input.customerGUID = this.customer?.customerGUID ?? AppConsts.ciscoParameters.customerId;
    });

    this.updateCaseInputDtoDiffer = this.differs.find(this.input).create();
    this.updateCaseForm.controls.caseCategory1.valueChanges.subscribe(values => this.autoSave());
    this.updateCaseForm.controls.caseCategory2.valueChanges.subscribe(values => this.autoSave());
    this.updateCaseForm.controls.caseCategory3.valueChanges.subscribe(values => this.autoSave());
    this.updateCaseForm.controls.caseTitle.valueChanges.subscribe(values => this.autoSave());
    this.updateCaseForm.controls.adHocCriteria.valueChanges.subscribe(values => this.autoSave());
    this.updateCaseForm.controls.caseDetails.valueChanges.subscribe(values => this.autoSave());
    this.updateCaseForm.controls.followupAction.valueChanges.subscribe(values => this.autoSave());
  }

  modelChanged(changes: KeyValueChanges<string, any>) {
    this.autoSave();
  }

  ngDoCheck(): void {
    const changes = this.updateCaseInputDtoDiffer.diff(this.input);
    if (changes) {
      this.modelChanged(changes);
    }
  }

  ngOnDestroy() {
    this.checkCustomerGuidSubscription.unsubscribe();
  }

  submitCallback() {
    this.input.followupRequired = true;
    this.input.caseStatus = CaseStatusInputEnum.InProgress;
    this.input.resolution = null || '';
    if (!this.updateCaseForm.controls.followupAction.value) {
      // abp.message.warn('Please fill in follow-up action for a callback','Validation Error');
      // return false;
    }
    this.updateCase();
  }

  submitNoCallback() {
    this.input.followupRequired = false;
    this.input.caseStatus = CaseStatusInputEnum.InProgress;
    this.input.resolution = null || '';
    this.updateCase();
  }

  submitResolveCase() {
    this.input.followupRequired = false;
    this.input.caseStatus = CaseStatusInputEnum.Resolved;
    this.input.resolution = "Resolved on Webex";
    this.updateCase();
  }

  updateCase() {

    this.input.caseID = AppConsts.ciscoParameters.caseId;

    if (!this.input.caseID) {
      abp.message.error('Missing Case ID')
      return;
    }

    this.input.customerGUID = this.customer?.customerGUID ?? AppConsts.ciscoParameters.customerId;
    if (!this.input.customerGUID) {
      abp.message.error('Please Assign a customer', 'Missing Customer');
      return;
    }

    // this.input.caseTitle //viewbound
    // this.input.caseDetails;//viewbound
    this.input.systemAuthenticated = AppConsts.ciscoParameters.customerIsAuthenticated;
    this.input.manualVerification = this.input.manualVerification;
    this.input.ipccCallExtensionID = AppConsts.ciscoParameters.iPCCCallExtensionID;//.replace(/-/g, '').substring(0,32);
    this.input.owner = AppConsts.ciscoParameters.agentUsername;
    // cat1,cat2,cat3 viewbound
    // adhoc viewbound
    if (!this.updateCaseForm.controls.followupAction.value) {
      this.input.followupAction = '-';
    } else {
      this.input.followupAction = this.updateCaseForm.controls.followupAction.value;
    }
    this.input.modifiedBy = AppConsts.ciscoParameters.agentUsername;
    // followupRequired,caseStatus set already,
    // resolution set sometimes
    this.input.triggeredFrom = this.screenName;
    this.input.triggeredBy = AppConsts.ciscoParameters.agentName;

    this._ai.trackEvent('UpdateCase - Data', { data: this.input.toJSON() });
    this._ai.startTrackEvent('UpdateCase');
    this.loading = true;
    this._sVCSConnectorServiceProxy
      .updateCase(AppConsts.ciscoParameters.apiToken,this.input)
      .pipe(
        finalize(() => {
          this._ai.stopTrackEvent('UpdateCase');
          this.loading = false;
        })
      )
      .subscribe((result) => {
        this.loading = false;
        abp.message.success("Case Updated");
        this.disableForm();
      });
  }

  disableForm() {
    this.caseUpdatedEvent.emit(true);
    this.updateCaseForm.disable();
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
        this.updateCaseForm.controls.caseCategory1.setValue(null);
        this.cat2Options = this.dropdownData.categories.filter(x => x.type === AppConsts.categorySelect.Category2);
      case 2:
        this.input.caseCategory2 = '';
        this.updateCaseForm.controls.caseCategory2.setValue(null);
        this.cat3Options = this.dropdownData.categories.filter(x => x.type === AppConsts.categorySelect.Category3);
      // this.updateCaseForm.controls.caseCategory3.disable();
      case 3:
        this.input.caseCategory3 = '';
        this.updateCaseForm.controls.caseCategory3.setValue(null);
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
        this.updateCaseForm.controls.caseCategory2.setValue(null);

        this.updateCaseForm.controls.caseCategory2.enable();
        this.updateCaseForm.controls.caseCategory3.disable();
      case 2:
        this.cat3Options = this.dropdownData.categories.filter(x => x.relatedCategory2ID == $event.categoryId && x.type === AppConsts.categorySelect.Category3)
        this.input.caseCategory3 = '';
        this.updateCaseForm.controls.caseCategory3.setValue(null);

        this.updateCaseForm.controls.caseCategory3.enable();

        if (!this.input.caseCategory1) {
          this.input.caseCategory1 = $event.relatedCategory1ID;
          this.updateCaseForm.controls.caseCategory1.setValue(this.input.caseCategory1);
        }

        break;

      case 3:
        this.cat2Options = this.dropdownData.categories.filter(x => x.relatedCategory1ID == $event.relatedCategory1ID && x.type === AppConsts.categorySelect.Category2)
        this.input.caseCategory1 = $event.relatedCategory1ID;
        this.input.caseCategory2 = $event.relatedCategory2ID;
        this.updateCaseForm.controls.caseCategory1.setValue(this.input.caseCategory1);
        this.updateCaseForm.controls.caseCategory2.setValue(this.input.caseCategory2);
        this.updateCaseForm.controls.caseCategory1.enable();
        this.updateCaseForm.controls.caseCategory2.enable();
        break;
      default:
        break;
    }
  }

  @debounce_or_throttle(5000, false, 4500)
  autoSave() {
    if (this.updateCaseForm.disabled) {
      return;
    }

    var autoSaveObject: UpdateCaseAutoSaveInputDto = new UpdateCaseAutoSaveInputDto();

    this.input.caseID = AppConsts.ciscoParameters.caseId;
    if (!this.input.caseID) {
      abp.notify.warn('No Case Id', 'Unable to autosave');
      return;
    }

    autoSaveObject.customerGUID = this.customer?.customerGUID ?? AppConsts.ciscoParameters.customerId;
    if (!autoSaveObject.customerGUID) {
      abp.notify.warn('Please Assign a customer', 'Unable to autosave');
      return;
    }

    autoSaveObject.caseTitle = this.input.caseTitle;
    autoSaveObject.caseDetails = this.input.caseDetails;
    autoSaveObject.systemAuthenticated = AppConsts.ciscoParameters.customerIsAuthenticated;
    autoSaveObject.manualVerification = this.input.manualVerification;
    autoSaveObject.ipccCallExtensionID = AppConsts.ciscoParameters.iPCCCallExtensionID;//.replace(/-/g, '').substring(0,32);
    autoSaveObject.owner = AppConsts.ciscoParameters.agentUsername;
    autoSaveObject.caseCategory1 = this.input.caseCategory1;
    autoSaveObject.caseCategory2 = this.input.caseCategory2;
    autoSaveObject.caseCategory3 = this.input.caseCategory3;
    autoSaveObject.adHocCriteria = this.input.adHocCriteria;

    if (!this.updateCaseForm.controls.followupAction.value) {
      autoSaveObject.followupAction = '-';
    } else {
      autoSaveObject.followupAction = this.updateCaseForm.controls.followupAction.value;
    }
    autoSaveObject.modifiedBy = AppConsts.ciscoParameters.agentUsername;
    autoSaveObject.followupRequired = this.input.followupRequired;
    autoSaveObject.caseStatus = this.input.caseStatus;
    autoSaveObject.resolution = this.input.resolution;
    autoSaveObject.triggeredFrom = this.screenName;
    autoSaveObject.triggeredBy = AppConsts.ciscoParameters.agentName;

    this._ai.trackEvent('AutoSaveCase - Data', { caseId: this.input.caseID, data: autoSaveObject.toJSON() });
    this._ai.startTrackEvent('AutoSaveCase');
    this.autoSaveInProgress = true;
    this._autoSaveCaseServiceProxy
      .autoSaveCase(this.input.caseID, AppConsts.ciscoParameters.apiToken, autoSaveObject)
      .pipe(
        finalize(() => {
          this._ai.stopTrackEvent('AutoSaveCase');
          this.autoSaveInProgress = false;
        })
      )
      .subscribe((result) => {
        // nothing to do here
      }, error => {
        this._ai.trackEvent('AutoSaveCase Failed - Data', { caseId: this.input.caseID, data: autoSaveObject.toJSON(), error: error });
      });
  }
}