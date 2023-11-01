import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AppInsightsService } from '@markpieszak/ng-application-insights';
import { AdminServiceProxy, StatusFlagDto } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-status-flags',
  templateUrl: './status-flags.component.html',
  styleUrls: ['./status-flags.component.css']
})
export class StatusFlagsComponent implements OnInit {

  adminToken: string;
  public loading = false;

  public statusFlags: StatusFlagDto[] = [];

  updateStatusFlagForm = new FormGroup({
    code: new FormControl('', [Validators.required]),
    description: new FormControl('', [Validators.required]),
  });

  constructor(
    private route: ActivatedRoute,
    private _ai: AppInsightsService,
    private _adminServiceProxy: AdminServiceProxy
  ) { }

  ngOnInit(): void {
    this.adminToken = this.route.parent.snapshot.paramMap.get('AdminToken') || '';
    this.reload();
  }

  reload() {
    this._ai.startTrackEvent('getStatusFlags');
    this.loading = true;
    this._adminServiceProxy
      .getStatusFlags(this.adminToken)
      .pipe(
        finalize(() => {
          this.loading = false;
          this._ai.stopTrackEvent('getStatusFlags');
        })
      )
      .subscribe((result) => {
        this.statusFlags = result;
        this._ai.trackMetric('getStatusFlags Complete', this.statusFlags.length, null, null, null, { "AdminToken": this.adminToken });
      });
  }

  onSubmit() {
    this.loading = true;

    var inputData = new StatusFlagDto({
      code: this.updateStatusFlagForm.controls.code.value.toString().trim().toUpperCase(),
      description: this.updateStatusFlagForm.controls.description.value
    });

    this._ai.trackEvent('upsertStatusFlags - Data', { data: inputData.toJSON() });
    this._ai.startTrackEvent('upsertStatusFlags');

    this._adminServiceProxy
      .upsertStatusFlags(this.adminToken, inputData)
      .pipe(
        finalize(() => {
          this.loading = false;
          this._ai.stopTrackEvent('upsertStatusFlags');
        })
      )
      .subscribe(() => {
        abp.notify.success(`StatusFlag ${inputData.code} added/updated!`);
        this.reload();
      });
  }

  edit(statusFlag: StatusFlagDto){
    this.updateStatusFlagForm.controls.code.setValue(statusFlag.code);
    this.updateStatusFlagForm.controls.description.setValue(statusFlag.description);
  }

  delete(statusFlag: StatusFlagDto){
    var $this = this;
    abp.message.confirm(
      `Are you sure you want to delete StatusFlag code '${statusFlag.code}'?`,
      'Confirm Deletion',
      function (isConfirmed) {
        if (isConfirmed) {
          $this.loading = true;
          $this._ai.trackEvent('deleteStatusFlags - Data', { data: statusFlag.code });
          $this._ai.startTrackEvent('deleteStatusFlags');
          $this._adminServiceProxy
            .deleteStatusFlags(statusFlag.code, $this.adminToken)
            .pipe(
              finalize(() => {
                $this.loading = false;
                $this._ai.stopTrackEvent('deleteStatusFlags');
                abp.notify.success(`StatusFlag ${statusFlag.code} deleted!`);
              })
            )
            .subscribe(() => {
              $this.reload();
            });
        }
      }
    );
  }

}
