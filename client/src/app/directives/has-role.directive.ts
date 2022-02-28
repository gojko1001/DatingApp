import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs/operators';
import { User } from '../model/user';
import { AccountService } from '../services/account.service';

@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];
  user: User;

  constructor(private viewContainerRef: ViewContainerRef, 
    private templateRef: TemplateRef<any>, 
    private accountService: AccountService) {
      this.accountService.currentUser$.pipe(take(1)).subscribe(data => {
        this.user = data;
      });
     }

  ngOnInit(): void {
    // clear view if no roles
    if(this.user == null || !this.user?.roles){
      this.viewContainerRef.clear();
      return;
    }

    if(this.user?.roles.some(r => this.appHasRole.includes(r))){
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainerRef.clear();
    }
  }

}
