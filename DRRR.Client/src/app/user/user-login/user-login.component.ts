import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { SystemMessagesService } from '../../core/services/system-messages.service';
import { FormErrorsAutoClearerService } from '../../core/services/form-errors-auto-clearer.service';
import { UserLoginService } from './user-login.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  templateUrl: './user-login.component.html',
  styleUrls: ['./user-login.component.css']
})
export class UserLoginComponent implements OnInit {

  loginForm: FormGroup;

  formErrorMessages: object;

  private validationMessages: { [key: string]: Function };

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private msg: SystemMessagesService,
    private loginService: UserLoginService,
    private autoClearer: FormErrorsAutoClearerService,
    private auth: AuthService,
    private toastr: ToastrService) { }

  ngOnInit () {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
    this.formErrorMessages = {};
    // 为避免获取消息时配置文件尚未加载，在外面多包一层函数
    this.validationMessages = {
      username: () => this.msg.getMessage('E001', '用户名'),
      password: () => this.msg.getMessage('E001', '密码')
    };
    this.autoClearer.register(this.loginForm, this.formErrorMessages);
    this.loginForm.valueChanges.subscribe(_ => {
      if (this.loginForm.valid) {
        this.formErrorMessages['username'] = '';
      }
    });
  }

  login(data: object) {
    if (!this.loginForm.valid) {
      for (const controlName in this.loginForm.controls) {
        if (!this.loginForm.controls[controlName].valid) {
          this.formErrorMessages[controlName] = this.validationMessages[controlName]();
        }
      }
    } else if (!this.formErrorMessages['username']) {
      this.loginService
        .login(data)
        .subscribe(res => {
          if (res.error) {
            // 在用户名输入框下方显示错误信息
            this.formErrorMessages['username'] = res.error;
          }else {
            this.auth.saveAccessToken(res.accessToken);
            this.auth.saveRefreshToken(res.refreshToken);
            this.router.navigate(['/rooms', {page: 1}]);
            this.toastr.success(this.msg.getMessage('I001', '登录'));
          }
      });
    }
  }
}
