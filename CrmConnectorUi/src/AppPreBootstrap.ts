import { CompilerOptions, NgModuleRef, Type } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { environment } from './environments/environment';
import { Injector } from '@angular/core';
import { XmlHttpRequestHelper } from './shared/helpers/XmlHttpRequestHelper';
import { AppConsts } from './shared/AppConsts';
import { NgxSpinnerService } from 'ngx-spinner';
import { AppInsightsService } from '@markpieszak/ng-application-insights';

export class AppPreBootstrap {
    static run(appRootUrl: string, injector: Injector, callback: () => void, resolve: any, reject: any): void {
        let spinnerService = injector.get(NgxSpinnerService);
        AppPreBootstrap.getApplicationConfig(appRootUrl, injector, () => {
            spinnerService.hide();
            resolve(true);
        });
    }

    static bootstrap<TM>(
        moduleType: Type<TM>,
        compilerOptions?: CompilerOptions | CompilerOptions[]
    ): Promise<NgModuleRef<TM>> {
        return platformBrowserDynamic().bootstrapModule(moduleType, compilerOptions);
    }

    private static getApplicationConfig(appRootUrl: string, injector: Injector, callback: () => void) {
        let type = 'GET';
        let url = appRootUrl + 'assets/' + environment.appConfig;
        // let customHeaders = [
        //     {
        //         name: abp.multiTenancy.tenantIdCookieName,
        //         value: abp.multiTenancy.getTenantIdCookie() + '',
        //     },
        // ];

        XmlHttpRequestHelper.ajax(type, url, {}, null, (result: any) => {
            AppConsts.title = result.title;
            AppConsts.appBaseUrlFormat = result.appBaseUrl;
            AppConsts.remoteServiceBaseUrlFormat = result.remoteServiceBaseUrl;
            AppConsts.appBaseUrl = result.appBaseUrl;
            AppConsts.remoteServiceBaseUrl = result.remoteServiceBaseUrl;
            AppConsts.AvailableLatestVersion = result.uiVersion;
            AppConsts.InstrumentationKey = result.instrumentationKey;

            let appInsightsService = injector.get(AppInsightsService);
            appInsightsService.config = {
                instrumentationKey: AppConsts.InstrumentationKey
            };
            appInsightsService.init();
            callback();
        });
    }
}
