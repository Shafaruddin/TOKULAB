import { Component, EventEmitter, Input, OnInit, Output, ViewEncapsulation } from '@angular/core';
import { GetCustomerOutputDto } from '@shared/service-proxies/service-proxies';
import { AppInsightsService } from '@markpieszak/ng-application-insights';

@Component({
  selector: 'app-customer-information',
  templateUrl: './customer-information.component.html',
  styleUrls: ['./customer-information.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class CustomerInformationComponent implements OnInit {

  @Input() public customer: GetCustomerOutputDto | null;
  @Input()
  set caseUpdated(value: boolean) {
    if (event) {
      this.formDisabled = value;
    }
  }

  @Output() manualAuthenticateChange = new EventEmitter<boolean>();

  public isManualVerification: boolean = false;
  public formDisabled: boolean = false;

  constructor(private _ai: AppInsightsService,) { }

  ngOnInit(): void {

  }

  checkValue() {
    this.manualAuthenticateChange.emit(this.isManualVerification);
  }

}
