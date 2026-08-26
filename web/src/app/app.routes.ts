import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'books' },
  { path: 'login', loadComponent: () => import('./pages/login').then((m) => m.Login) },
  { path: 'register', loadComponent: () => import('./pages/register').then((m) => m.Register) },
  {
    path: 'books',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/book-list').then((m) => m.BookList),
  },
  {
    path: 'books/new',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/book-form').then((m) => m.BookForm),
  },
  {
    path: 'books/:id/edit',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/book-form').then((m) => m.BookForm),
  },
  {
    path: 'quotes',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/quote-list').then((m) => m.QuoteList),
  },
  { path: '**', redirectTo: 'books' },
];
