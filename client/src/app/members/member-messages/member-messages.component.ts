import {Component, Input, OnInit, ViewChild} from '@angular/core';
import {Message} from "../../_models/message";
import {CommonModule} from "@angular/common";
import {TimeagoModule} from "ngx-timeago";
import {MessageService} from "../../_services/message.service";
import {FormsModule, NgForm} from "@angular/forms";

@Component({
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

  constructor(public messageService: MessageService) {}

  sendMessage() {
    this.messageService.sendMessage(this.username, this.messageContent).then(() => {
      this.messageForm?.reset();
    });
  }
}
