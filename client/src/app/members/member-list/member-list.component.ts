import {Component, OnInit} from '@angular/core';
import {Member} from "../../_models/member";
import {MemberService} from "../../_services/member.service";
import {Observable, take} from "rxjs";
import {Pagination} from "../../_models/pagination";
import {UserParams} from "../../_models/userParams";

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members: Member[] = []
  pagination: Pagination | undefined
  userParams: UserParams | undefined
  genderList = [{value: "male", display: "Males"}, {value: "female", display: "Females"}]

  constructor(private memberService: MemberService) {
    this.userParams = memberService.getUserParams()
  }

  ngOnInit(): void {
    this.loadMembers()
  }

  loadMembers() {
    if (this.userParams) {
      this.memberService.setUserParams(this.userParams)
      this.memberService.getMembers(this.userParams).subscribe({
        next: response => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      });
    }
  }

  resetFilter() {
    this.userParams = this.memberService.resetUserParams()
    this.loadMembers();
  }

  pageChanged(event: any) {
    if (this.userParams && this.userParams?.pageNumber !== event.page) {
      this.userParams.pageNumber = event.page;
      this.memberService.setUserParams(this.userParams)
      this.loadMembers();
    }
  }
}
