import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import * as ApiServiceProxies from './service-proxies';
import { AbpHttpInterceptor } from 'abp-ng2-module';

@NgModule({
    providers: [
        ApiServiceProxies.AdminServiceProxy,        
        ApiServiceProxies.SVCSConnectorServiceProxy,
        ApiServiceProxies.AutoSaveCaseServiceProxy,
        { provide: HTTP_INTERCEPTORS, useClass: AbpHttpInterceptor, multi: true },
    ],
})
export class ServiceProxyModule {}
