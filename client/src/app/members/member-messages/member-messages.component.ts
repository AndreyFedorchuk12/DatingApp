import {ChangeDetectionStrategy, Component, Input, ViewChild} from '@angular/core';
import {CommonModule} from "@angular/common";
import {TimeagoModule} from "ngx-timeago";
import {MessageService} from "../../_services/message.service";
import {FormsModule, NgForm} from "@angular/forms";

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  standalone: true,
  templateUrl: './member-messages.component.html',
  imports: [
    CommonModule,
    TimeagoModule,
    FormsModule
  ],
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent{
  @ViewChild("messageForm") messageForm?: NgForm
  @Input() username = ""
  messageContent =""
  loading = false

  constructor(public messageService: MessageService) {}

  sendMessage() {
    if(!this.username) return
    this.loading = true;
    this.messageService.sendMessage(this.username, this.messageContent).then(() => {
      this.messageForm?.reset();
    }).finally(() => this.loading = false);
  }
}
