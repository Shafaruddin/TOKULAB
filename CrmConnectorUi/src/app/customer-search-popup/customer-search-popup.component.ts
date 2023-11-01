import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { AppConsts } from '@shared/AppConsts';
import { Customer, FindCustomerInputDto, FindCustomerOutputDto, SVCSConnectorServiceProxy } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';
import { PaginationInstance } from 'ngx-pagination';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AppInsightsService } from '@markpieszak/ng-application-insights';

@Component({
  selector: 'app-customer-search-popup',
  templateUrl: './customer-search-popup.component.html',
  styleUrls: ['./customer-search-popup.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class CustomerSearchPopupComponent implements OnInit {

  screenName = 'WEBFORM: Find Customer Popup';

  loading = false;

  data: FindCustomerOutputDto = new FindCustomerOutputDto({
    customers: [],
    totalRecordsFound: "0",
    returnStatus: null
  });

  findCustomerForm = new FormGroup({
    name: new FormControl('', [Validators.maxLength(338)]),
    accountNo: new FormControl('', [Validators.maxLength(9)]),
    email: new FormControl('', [Validators.email, Validators.maxLength(100)]),
    identificatioNo: new FormControl('', [Validators.maxLength(20)]),
    mobile: new FormControl('', [Validators.maxLength(20)]),
  });

  public maxSize: number = 7;
  public directionLinks: boolean = true;
  public autoHide: boolean = true;
  public responsive: boolean = true;
  public config: PaginationInstance = {
    id: 'customerSearchPagination',
    itemsPerPage: 5,
    currentPage: 1
  };
  public labels: any = {
    previousLabel: 'Previous',
    nextLabel: 'Next'
  };

  onPageChange(number: number) {
    this.config.currentPage = number;
  }

  onPageBoundsCorrection(number: number) {
    this.config.currentPage = number;
  }

  constructor(
    private _sVCSConnectorServiceProxy: SVCSConnectorServiceProxy,
    public activeModal: NgbActiveModal,
    private _ai: AppInsightsService,
  ) { }

  ngOnInit(): void {

  }

  dismiss(data: any) {
    this.activeModal.dismiss(data)
  }

  assignCustomer(customer: Customer) {
    this.activeModal.close(customer);
  }

  nullIfEmpty(str: string): string | null {
    if (!str) {
      return null;
    }
    return str;
  }

  onSubmit() {
    this.loading = true;

    var inputData = new FindCustomerInputDto({
      name: this.nullIfEmpty(this.findCustomerForm.controls.name.value),
      idNumber: this.nullIfEmpty(this.findCustomerForm.controls.identificatioNo.value),
      accountNumber: this.nullIfEmpty(this.findCustomerForm.controls.accountNo.value),
      mobileNo: this.nullIfEmpty(this.findCustomerForm.controls.mobile.value),
      emailAddress: this.nullIfEmpty(this.findCustomerForm.controls.email.value),
      triggeredFrom: this.screenName,
      triggeredBy: AppConsts.ciscoParameters.agentName
    });

    this._ai.trackEvent('FindCustomer - Data', { data: inputData.toJSON() });
    this._ai.startTrackEvent('FindCustomer');

    this._sVCSConnectorServiceProxy
      .findCustomer(AppConsts.ciscoParameters.apiToken,inputData)
      .pipe(
        finalize(() => {
          this.loading = false;
          this._ai.stopTrackEvent('FindCustomer');
        })
      )
      .subscribe((result) => {
        this.loading = false;
        this.data = result;
      });
  }

}
