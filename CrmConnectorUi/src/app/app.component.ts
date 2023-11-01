import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, Component, Inject, OnInit, PLATFORM_ID, ViewEncapsulation } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { AppConsts } from '@shared/AppConsts';
import { NgxSpinnerTextService } from '@shared/ngx-spinner-text.service';
import { ContactModeInputEnum } from '@shared/service-proxies/service-proxies';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class AppComponent implements OnInit, AfterViewInit {

  ngxSpinnerText: NgxSpinnerTextService;

  public constructor(
    private titleService: Title,
    private route: ActivatedRoute,
    _ngxSpinnerText: NgxSpinnerTextService,
    @Inject(PLATFORM_ID) private platformId,
  ) {
    AppConsts.isBrowser = isPlatformBrowser(this.platformId);
    this.ngxSpinnerText = _ngxSpinnerText;
    this.titleService.setTitle(AppConsts.title);
  }

  ngAfterViewInit(): void {

  }

  ngOnInit(): void {
    
    console.info('Running Version', AppConsts.WebAppGuiVersion);
    console.info('Available Version', AppConsts.AvailableLatestVersion);

    if (AppConsts.WebAppGuiVersion != AppConsts.AvailableLatestVersion) {
      abp.message.confirm(
        'Update now to latest version?',
        'Old version detected',
        function (isConfirmed) {
          if (isConfirmed) {
            window.location.reload();
          }
        }
      );
    }

  }

  getSpinnerText(): string {
    return this.ngxSpinnerText.getText();
  }
}
