import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppInsightsService } from '@markpieszak/ng-application-insights';
import { AdminServiceProxy } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-cache',
  templateUrl: './cache.component.html',
  styleUrls: ['./cache.component.css']
})
export class CacheComponent implements OnInit {

  adminToken: string;
  public loading = false;
  public caches = [
    'Retailers', 'StatusFlags', 'CategoryAndAdhocCriteria'
  ]

  constructor(
    private route: ActivatedRoute,
    private _ai: AppInsightsService,
    private _adminServiceProxy: AdminServiceProxy
  ) { }

  ngOnInit(): void {
    this.adminToken = this.route.parent.snapshot.paramMap.get('AdminToken') || '';
  }

  clearCache(cacheName: string) {
    var $this = this;

    var method;
    switch (cacheName) {
      case 'Retailers':
        method = this._adminServiceProxy.clearRetailersCache;
        break;
      case 'StatusFlags':
        method = this._adminServiceProxy.clearStatusFlagsCache;
        break;
      case 'CategoryAndAdhocCriteria':
        method = this._adminServiceProxy.clearCategoryAndAdhocCriteriaCache
        break;
      default:
        alert('Invalid Param');
        return;
    }

    abp.message.confirm(
      `Are you sure you want to clear the '${cacheName}' cache?`,
      'Confirm Deletion',
      function (isConfirmed) {
        if (isConfirmed) {
          $this.loading = true;
          $this._ai.startTrackEvent(`clear${cacheName}Cache`);
          switch (cacheName) {
            case 'Retailers':
              $this._adminServiceProxy.clearRetailersCache($this.adminToken)
                .pipe(
                  finalize(() => { $this.loading = false; $this._ai.stopTrackEvent(`clear${cacheName}Cache`); })
                )
                .subscribe(() => {
                  abp.notify.success(`${cacheName} cache cleared!`);
                });
              break;
            case 'StatusFlags':
              $this._adminServiceProxy.clearStatusFlagsCache($this.adminToken)
                .pipe(
                  finalize(() => { $this.loading = false; $this._ai.stopTrackEvent(`clear${cacheName}Cache`); })
                )
                .subscribe(() => {
                  abp.notify.success(`${cacheName} cache cleared!`);
                });
              break;
            case 'CategoryAndAdhocCriteria':
              $this._adminServiceProxy.clearCategoryAndAdhocCriteriaCache($this.adminToken)
                .pipe(
                  finalize(() => { $this.loading = false; $this._ai.stopTrackEvent(`clear${cacheName}Cache`); })
                )
                .subscribe(() => {
                  abp.notify.success(`${cacheName} cache cleared!`);
                });
              break;
            default:
              alert('Invalid Param');
              return;
          }
        }
      }
    );
  }

}
