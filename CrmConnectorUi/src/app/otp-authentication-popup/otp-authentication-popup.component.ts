import { Component, Input, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ChallengeOTPInputDto, Customer, FindCustomerInputDto, GenerateOTPInputDto, GenerateOTPOutputDto, SVCSConnectorServiceProxy } from '@shared/service-proxies/service-proxies';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AppConsts } from '@shared/AppConsts';
import { finalize } from 'rxjs/operators';
import { CountdownModule, CountdownComponent } from 'ngx-countdown';
import { AppPreBootstrap } from 'AppPreBootstrap';

@Component({
  selector: 'app-otp-authentication-popup',
  templateUrl: './otp-authentication-popup.component.html',
  styleUrls: ['./otp-authentication-popup.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class OtpAuthenticationPopupComponent implements OnInit {

  public appConsts = AppConsts;

  @Input() public mobile: string;

  screenName = 'WEBFORM: OTP Authentication Popup';
  public otp: string = '';

  public loading = false;

  data: GenerateOTPOutputDto = new GenerateOTPOutputDto();

  @ViewChild('ngOtpInput') ngOtpInputRef: any;
  @ViewChild('cd', { static: false }) private countdown: CountdownComponent;
  public countdownConfig = {
    demand: true,
    leftTime: 30,
    format: 'mm:ss'
  }

  validateOtpForm = new FormGroup({
    otpValidate: new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(6)])
  });

  handleCountDownEvent($event) {
    if ($event.action === 'done') {
      abp.notify.warn('OTP expired');
      this.activeModal.dismiss('error');
    }
  }

  constructor(
    private _sVCSConnectorServiceProxy: SVCSConnectorServiceProxy,
    public activeModal: NgbActiveModal
  ) { }

  ngOnInit(): void {
    this.loading = true;
    // this.ngOtpInputRef.setValue(12345);

    var inputData = new GenerateOTPInputDto({
      mobile: this.mobile,
      triggeredFrom: this.screenName,
      triggeredBy: AppConsts.ciscoParameters.agentName
    });

    this._sVCSConnectorServiceProxy
      .generateOTP(AppConsts.ciscoParameters.apiToken,inputData)
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe((result) => {
        this.data = result;
        if (this.data.otpSent) {
          this.countdown.left = this.data.secondsRemaining * 1000;
          this.countdown.begin();
        }
      }, (error) => {
        this.activeModal.dismiss('error');
      });
  }

  dismiss(data: any) {
    this.activeModal.dismiss(data)
  }

  nullIfEmpty(str: string): string | null {
    if (!str) {
      return null;
    }
    return str;
  }

  otpValue() {
    return this.otp
  }

  onOtpChange(otp) {
    this.otp = otp;
  }

  onSubmit() {
    this.loading = true;

    var inputData = new ChallengeOTPInputDto({
      mobile: this.mobile,
      otpValidate: this.nullIfEmpty(this.otp),
      triggeredFrom: this.screenName,
      triggeredBy: AppConsts.ciscoParameters.agentName
    });

    this._sVCSConnectorServiceProxy
      .challengeOTP(AppConsts.ciscoParameters.apiToken,inputData)
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe((result) => {
        if(result === true){
          this.activeModal.close(true);
        }
      });
  }

}
