import { HomeComponent as MainHomeComponent } from './pages/home/home.component';
import { Routes } from '@angular/router';
import { PlatformComponent } from './pages/platform/platform.component';
import { DashboardComponent } from './pages/platform/dashboard/dashboard.component';
import { SignInComponent } from './pages/auth/sign-in/sign-in.component';
import { SignUpComponent } from './pages/auth/sign-up/sign-up.component';
import { UsersComponent } from './pages/platform/users/users.component';
import { FileHistoryComponent } from './pages/platform/file-history/file-history.component';
import { ConvertFilesComponent } from './pages/platform/convert-files/convert-files.component';
import { CreateNewTemplateComponent } from './pages/platform/create-new-template/create-new-template.component';
import { PredictFromFilesComponent } from './pages/platform/predict-from-files/predict-from-files.component';
import { ChatComponent } from './pages/platform/chat/chat.component';
import { ProfileComponent } from './pages/platform/profile/profile.component';
import { authGuard } from './guards/auth.guard';
import { ValidationRequestsComponent } from './pages/platform/validation-requests/validation-requests.component';
import { ResetPasswordComponent } from './auth/reset-password/reset-password.component';
import { ForgetPasswordComponent } from './auth/forget-password/forget-password.component';

export const routes: Routes = [
  {
    path: '',
    component: MainHomeComponent
  },
  {
    path: 'auth',
    children: [
      {
        path: 'sign-in',
        component: SignInComponent
      },
      {
        path: 'sign-up',
        component: SignUpComponent
      },
      {
        path: 'forget-password',
        component: ForgetPasswordComponent
      },
      {
        path: 'reset-password',
        component: ResetPasswordComponent
      },
      {
        path: '**',
        redirectTo: 'sign-in'
      }
    ]
  },
  {
    path: 'platform',
    component: PlatformComponent,
    canActivate: [authGuard], // ce guard est utilisé pour protéger n'importe quelle route sous /platform/
    children: [
      {
        path: 'home',
        component: DashboardComponent
      },
      {
        path: 'profile',
        component: ProfileComponent
      },
      {
        path: 'users',
        component: UsersComponent
      },
      {
        path: 'file-history',
        component: FileHistoryComponent
      },
      {
        path: 'convert-files',
        component: ConvertFilesComponent
      },
      {
        path: 'validation-requests',
        component: ValidationRequestsComponent
      },
      {
        path: 'create-new-template',
        component: CreateNewTemplateComponent
      },
      {
        path: 'predict-from-files',
        component: PredictFromFilesComponent
      },
      {
        path: 'chat',
        component: ChatComponent
      },
      {
        path: '**',
        redirectTo: 'home'
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
