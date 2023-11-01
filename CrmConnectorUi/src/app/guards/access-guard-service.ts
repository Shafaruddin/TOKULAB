import { Inject, Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRoute } from '@angular/router';
import { isDevMode } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { AppConsts } from '@shared/AppConsts';

@Injectable()
export class AccessGuardService implements CanActivate {

    constructor(
        @Inject(DOCUMENT) private document: Document,
        public router: Router,
        private route: ActivatedRoute) {
    }

    getQueryVar(varName){
        // Grab and unescape the query string - appending an '&' keeps the RegExp simple
        // for the sake of this example.
        var queryStr = unescape(window.location.search) + '&';
    
        // Dynamic replacement RegExp
        var regex = new RegExp('.*?[&\\?]' + varName + '=(.*?)&.*');
    
        // Apply RegExp to the query string
        var val = queryStr.replace(regex, "$1");
    
        // If the string is the same, we didn't find a match - return false
        return val == queryStr ? false : val;
    }

    canActivate(): boolean {
        if(AppConsts.ciscoParameters.apiToken.startsWith('Basic')){
            return true;
        }
        if(!this.getQueryVar('authToken')) {
            abp.message.error('Missing Authentication Token', 'Access Error');
            return false;
        }
        AppConsts.ciscoParameters.apiToken = `Basic ${this.getQueryVar('authToken') || ""}`;
        return true;
    }
}