import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="row justify-content-center">
      <div class="col-12 col-md-6 col-lg-5">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <h1 class="h4 mb-4"><i class="fa-solid fa-right-to-bracket me-2 text-primary"></i>Log in</h1>

            @if (error()) {
              <div class="alert alert-danger py-2">{{ error() }}</div>
            }

            <form (ngSubmit)="submit()" #f="ngForm">
              <div class="mb-3">
                <label class="form-label">Username</label>
                <input class="form-control" name="username" [(ngModel)]="username" required autofocus />
              </div>
              <div class="mb-3">
                <label class="form-label">Password</label>
                <input type="password" class="form-control" name="password" [(ngModel)]="password" required />
              </div>
              <button class="btn btn-primary w-100" [disabled]="loading() || f.invalid">
                @if (loading()) { <span class="spinner-border spinner-border-sm me-2"></span> }
                Log in
              </button>
            </form>

            <p class="text-center mt-3 mb-0 small">
              No account? <a routerLink="/register">Register</a>
            </p>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);

  username = '';
  password = '';
  loading = signal(false);
  error = signal('');

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    this.auth.login(this.username, this.password).subscribe({
      next: () => this.router.navigate(['/books']),
      error: (err) => {
        this.error.set(err?.error ?? 'Login failed. Check your credentials.');
        this.loading.set(false);
      },
    });
  }
}
