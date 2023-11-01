import { APP_INITIALIZER, CUSTOM_ELEMENTS_SCHEMA, Injector, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ToastrModule } from 'ngx-toastr';
import { HttpClientModule } from '@angular/common/http';
import { LoadingBarHttpClientModule } from '@ngx-loading-bar/http-client';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AutocompleteLibModule } from 'angular-ng-autocomplete';
import { AutosizeModule } from '@techiediaries/ngx-textarea-autosize';
import { UppercaseDirective } from './directives/uppercase.directive';
import { NgSelectModule } from '@ng-select/ng-select';
import { HomeComponent } from './home/home.component';
import { CustomerInformationComponent } from './customer-information/customer-information.component';
import { CustomerCaseActivityHistoryComponent } from './customer-case-activity-history/customer-case-activity-history.component';

import { PlatformLocation } from '@angular/common';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { AppConsts } from '@shared/AppConsts';
import { AppPreBootstrap } from 'AppPreBootstrap';
import { AdminServiceProxy, API_BASE_URL, SVCSConnectorServiceProxy } from '@shared/service-proxies/service-proxies';
import { UtilsModule } from 'utils/utils.module';
import { trigger } from '@angular/animations';
import { CustomerSearchPopupComponent } from './customer-search-popup/customer-search-popup.component';
import { ServiceProxyModule } from '@shared/service-proxies/service-proxy.module';
import { AppSharedModule } from '@shared/app-shared.module';
import { NgxPaginationModule } from 'ngx-pagination';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgOtpInputModule } from  'ng-otp-input';
import { OtpAuthenticationPopupComponent } from './otp-authentication-popup/otp-authentication-popup.component';
import { CountdownModule } from 'ngx-countdown';
import { ApplicationInsightsModule, AppInsightsService } from '@markpieszak/ng-application-insights';
import { UpdateCaseComponent } from './update-case/update-case.component';
import { CreateCaseComponent } from './create-case/create-case.component';
import { CreateCaseFormComponent } from './create-case-form/create-case-form.component';
import { AccessGuardService } from './guards/access-guard-service';
import { FrameGuardService } from './guards/frame-guard-service';
import { RedirectComponent } from './redirect/redirect.component';

export function appInitializerFactory(injector: Injector, platformLocation: PlatformLocation) {
  return () => {
      let spinnerService = injector.get(NgxSpinnerService);

      spinnerService.show();

      return new Promise<boolean>((resolve, reject) => {
          AppConsts.appBaseHref = getBaseHref(platformLocation);
          let appBaseUrl = getDocumentOrigin() + AppConsts.appBaseHref;

          AppPreBootstrap.run(
              appBaseUrl,
              injector,
              () => {
                resolve(true);
              },
              resolve,
              reject
          );
      });
  };
}

function getDocumentOrigin() {
  if (!document.location.origin) {
      return (
          document.location.protocol +
          '//' +
          document.location.hostname +
          (document.location.port ? ':' + document.location.port : '')
      );
  }

  return document.location.origin;
}

export function getRemoteServiceBaseUrl(): string {
  return AppConsts.remoteServiceBaseUrl;
}

export function getBaseHref(platformLocation: PlatformLocation): string {
  let baseUrl = platformLocation.getBaseHrefFromDOM();
  if (baseUrl) {
      return baseUrl;
  }

  return '/';
}

@NgModule({
  declarations: [
    AppComponent,
    UppercaseDirective,
    HomeComponent,
    RedirectComponent,
    CustomerInformationComponent,
    CustomerCaseActivityHistoryComponent,
    CustomerSearchPopupComponent,
    OtpAuthenticationPopupComponent,
    UpdateCaseComponent,
    CreateCaseComponent,
    CreateCaseFormComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    AppRoutingModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    ToastrModule.forRoot(),
    LoadingBarHttpClientModule,
    AutocompleteLibModule,
    AutosizeModule,
    NgSelectModule,
    NgxSpinnerModule,
    UtilsModule,
    NgxPaginationModule,
    AppSharedModule,
    NgbModule,
    NgOtpInputModule,
    CountdownModule,
    ApplicationInsightsModule.forRoot({
      instrumentationKeySetLater: true
    }),
    ServiceProxyModule
  ],
  providers: [
    { provide: API_BASE_URL, useFactory: getRemoteServiceBaseUrl },
    {
      provide: APP_INITIALIZER,
      useFactory: appInitializerFactory,
      deps: [Injector, PlatformLocation],
      multi: true,
    },
    AppInsightsService,
    AccessGuardService,
    FrameGuardService
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  bootstrap: [AppComponent]
})
export class AppModule { }
