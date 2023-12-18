import {Component, OnInit, ViewChild} from '@angular/core';
import {ActivatedRoute} from '@angular/router'
import {Member} from "../../_models/member";
import {MemberService} from "../../_services/member.service";
import {CommonModule} from "@angular/common";
import {TabDirective, TabsetComponent, TabsModule} from "ngx-bootstrap/tabs";
import {GalleryItem, GalleryModule, ImageItem} from "ng-gallery";
import {TimeagoModule} from "ngx-timeago";
import {MemberMessagesComponent} from "../member-messages/member-messages.component";
import {MessageService} from "../../_services/message.service";
import {Message} from "../../_models/message";

@Component({
  selector: 'app-member-detail',
  standalone: true,
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
  imports: [CommonModule, TabsModule, GalleryModule, TimeagoModule, MemberMessagesComponent]
})
export class MemberDetailComponent implements OnInit {
  @ViewChild("memberTabs", {static: true}) memberTabs?: TabsetComponent
  member: Member = {} as Member
  images: GalleryItem[] = []
  activeTab?: TabDirective
  messages: Message[] = []

  constructor(private memberService: MemberService, private route: ActivatedRoute, private messageService: MessageService) {
  }

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => this.member = data["member"]
    })

    this.route.queryParams.subscribe({
      next: params => {
        params["tab"] && this.selectTab(params["tab"])
      }
    })

    this.getImages()
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data
    if (this.activeTab.heading === "Messages") {
      this.loadMessages()
    }
  }

  loadMessages() {
    if(this.member) {
      this.messageService.getMessageThread(this.member.userName).subscribe({
        next: messages => this.messages = messages
      })
    }
  }

  getImages() {
    if (!this.member)
      return

    for (const photo of this.member.photos) {
      this.images.push(new ImageItem({src: photo.url, thumb: photo.url}))
    }
  }

  selectTab(heading: string) {
    if(this.memberTabs) {
      const tab = this.memberTabs.tabs.find(t => t.heading === heading);
      if (tab) {
        tab.active = true;
      }
    }
  }
}
