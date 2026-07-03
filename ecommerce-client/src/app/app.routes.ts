import { Routes } from '@angular/router';
import { MainLayout } from './layouts/main-layout/main-layout';
import { AdminLayout } from './layouts/admin-layout/admin-layout';
import { Home } from './pages/home/home';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { AdminCategories } from './pages/admin/categories/admin-categories';
import { AdminCategoryCreate } from './pages/admin/categories/admin-category-create';
import { AdminCategoryEdit } from './pages/admin/categories/admin-category-edit';
import { adminGuard } from './guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    component: MainLayout,
    children: [
      { path: '', component: Home },
      { path: 'login', component: Login },
      { path: 'register', component: Register },
    ]
  },
  {
    path: 'admin',
    component: AdminLayout,
    canActivate: [adminGuard],
    children: [
      { path: '', redirectTo: 'categories', pathMatch: 'full' },
      { path: 'categories', component: AdminCategories, data: { title: 'Kategoriler' } },
      { path: 'categories/new', component: AdminCategoryCreate, data: { title: 'Yeni Kategori' } },
      { path: 'categories/edit/:id', component: AdminCategoryEdit, data: { title: 'Kategori Düzenle' } },
    ]
  },
  { path: '**', redirectTo: '' }
];
