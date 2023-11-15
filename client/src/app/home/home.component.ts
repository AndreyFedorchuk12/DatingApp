import {Component, OnInit} from '@angular/core';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit{
  registerMode = false
  users: any

  constructor(private httpClient: HttpClient) {
  }

  ngOnInit() {}

  registerToggle() {
    this.registerMode = !this.registerMode
  }

  getUsers() {
    this.httpClient.get("https://localhost:7083/api/users").subscribe({
      next: response => this.users = response,
      error: error => console.log(error),
      complete: () => console.log("Get users request has been completed")
    })
  }

  cancelRegisterMode(event: boolean) {
    this.registerMode = event
  }
}
