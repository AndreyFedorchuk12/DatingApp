import { Component } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {environment} from "../../../environments/environment";

@Component({
  selector: 'app-test-error',
  templateUrl: './test-error.component.html',
  styleUrls: ['./test-error.component.css']
})
export class TestErrorComponent {
  baseUrl = environment.apiUrl
  validationErrors: string[] = []

  constructor(private httpClient: HttpClient) {}

  get404Error() {
    this.httpClient.get(this.baseUrl + "buggy/not-found").subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }

  get400Error() {
    this.httpClient.get(this.baseUrl + "buggy/bad-request").subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }

  get500Error() {
    this.httpClient.get(this.baseUrl + "buggy/server-error").subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }

  get401Error() {
    this.httpClient.get(this.baseUrl + "buggy/auth").subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }

  get400ValidationError() {
    this.httpClient.post(this.baseUrl + "account/register", {}).subscribe({
      next: response => console.log(response),
      error: error => {
        console.log(error)
        this.validationErrors = error
      }
    })
  }
}
