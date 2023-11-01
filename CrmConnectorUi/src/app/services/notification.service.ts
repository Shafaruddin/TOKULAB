import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
@Injectable({
    providedIn: 'root'
})
export class NotificationService {  
    constructor(public toastr: ToastrService ) { }
    notify(toastrObject: any){
        switch(toastrObject.severity) {
            case "success":
                this.successMessage(toastrObject.detail);
                break;
            case "error":
                this.errorMessage(toastrObject.detail);
                break;
            case "warn":
                this.warningMessage(toastrObject.detail);
                break;
            default :
                this.toastr.show(toastrObject);
            }
    }
    successMessage(message: string) {
        this.toastr.success(message,'Success');
    }
    errorMessage(message: string) {
        this.toastr.error(message,'Error');
    }
    warningMessage(message: string) {
        this.toastr.warning(message,'Warning');
    }
}