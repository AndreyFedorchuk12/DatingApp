import {Component, Input} from '@angular/core';
import {Member} from "../../_models/member";
import {MemberService} from "../../_services/member.service";
import {ToastrService} from "ngx-toastr";
import {PresenceService} from "../../_services/presence.service";

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent {
  @Input() member: Member | undefined

  constructor(private memberService: MemberService, private toasts: ToastrService, public presenceService: PresenceService) {
  }

  addLike(member: Member) {
    this.memberService.addLike(member.userName).subscribe({
      next: _ => this.toasts.success('You have liked ' + member.knownAs)
    });
  }
}
