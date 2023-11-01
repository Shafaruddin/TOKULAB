import { Inject, Injectable } from '@angular/core';
import { Router, CanActivate } from '@angular/router';
import { isDevMode } from '@angular/core';
import { DOCUMENT } from '@angular/common';

@Injectable()
export class FrameGuardService implements CanActivate {

    private window: Window;

    constructor(
        @Inject(DOCUMENT) private document: Document,
        public router: Router) {
        this.window = this.document.defaultView;
    }

    canActivate(): boolean {
        if (isDevMode()) {
            return true;
        }
        if (window == window.top) {
            abp.message.error('Direct URL access is prohibited', 'Access Error');
            return false;
        }
        return true;
    }
}