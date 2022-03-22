import { UserService } from 'src/app/core/service/user.service';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {OrganizationService} from "../../core/service/organization.service";
@Component({
  selector: 'app-signin',
  templateUrl: './signin.component.html',
  styleUrls: ['./signin.component.scss'],
})
export class SigninComponent implements OnInit {
  loginForm: FormGroup;
  submitted = false;
  returnUrl: string;
  error = '';
  hide = true;
  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private userService: UserService,
    private organizationService: OrganizationService
  ) {}
  ngOnInit() {
    this.loginForm = this.formBuilder.group({
      username: ['fspies0@hotmail.com', Validators.required],
      password: ['c0mp2q', Validators.required],
    });
  }
  get f() {
    return this.loginForm.controls;
  }
  onSubmit() {

    console.error("Logging in");
    this.submitted = true;
    this.error = '';
    if (this.loginForm.invalid) {
      this.error = 'Username and Password not valid !';
      return;
    } else {
      this.userService
        .login(this.f.username.value, this.f.password.value)
        .subscribe({
          next: (res) => {
            if (res) {
              const token = this.userService.currentUserValue.token;
              if (token) {
                this.organizationService.list().subscribe();
                this.organizationService.getCurrent().subscribe();
                this.router.navigate(['/dashboard/main']);
              }
            } else {
              this.error = 'Invalid Login';
            }
          },
          error: (msg) => {
            console.error(msg);
            this.error = msg;
            this.submitted = false;
          }
        });
    }
  }
}
