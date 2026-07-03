import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';

// Hangi adres hangi sayfayı gösterecek?
export const routes: Routes = [
  { path: '', component: Home },          // "/" -> ana sayfa
  { path: 'login', component: Login },    // "/login" -> giriş
  { path: 'register', component: Register }, // "/register" -> kayıt
  { path: '**', redirectTo: '' }          // tanımsız adres -> ana sayfaya dön
];
