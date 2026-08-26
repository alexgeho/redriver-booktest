import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="row justify-content-center">
      <div class="col-12 col-md-6 col-lg-5">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <h1 class="h4 mb-4"><i class="fa-solid fa-user-plus me-2 text-primary"></i>Register</h1>

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
                <input type="password" class="form-control" name="password" [(ngModel)]="password"
                       required minlength="6" />
                <div class="form-text">At least 6 characters.</div>
              </div>
              <button class="btn btn-primary w-100" [disabled]="loading() || f.invalid">
                @if (loading()) { <span class="spinner-border spinner-border-sm me-2"></span> }
                Create account
              </button>
            </form>

            <p class="text-center mt-3 mb-0 small">
              Already have an account? <a routerLink="/login">Log in</a>
            </p>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class Register {
  private auth = inject(AuthService);
  private router = inject(Router);

  username = '';
  password = '';
  loading = signal(false);
  error = signal('');

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    this.auth.register(this.username, this.password).subscribe({
      next: () => this.router.navigate(['/books']),
      error: (err) => {
        this.error.set(err?.error ?? 'Registration failed.');
        this.loading.set(false);
      },
    });
  }
}
