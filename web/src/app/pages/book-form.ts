import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookService, BookInput } from '../services/book.service';

@Component({
  selector: 'app-book-form',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="row justify-content-center">
      <div class="col-12 col-md-8 col-lg-6">
        <div class="d-flex align-items-center mb-3">
          <a class="btn btn-sm btn-outline-secondary me-3" routerLink="/books">
            <i class="fa-solid fa-arrow-left"></i>
          </a>
          <h1 class="h4 mb-0">{{ isEdit() ? 'Edit book' : 'Add new book' }}</h1>
        </div>

        @if (error()) {
          <div class="alert alert-danger py-2">{{ error() }}</div>
        }

        <div class="card shadow-sm">
          <div class="card-body p-4">
            <form (ngSubmit)="submit()" #f="ngForm">
              <div class="mb-3">
                <label class="form-label">Title</label>
                <input class="form-control" name="title" [(ngModel)]="model.title" required autofocus />
              </div>
              <div class="mb-3">
                <label class="form-label">Author</label>
                <input class="form-control" name="author" [(ngModel)]="model.author" />
              </div>
              <div class="mb-3">
                <label class="form-label">Published date</label>
                <input type="date" class="form-control" name="publishedDate" [(ngModel)]="model.publishedDate" />
              </div>
              <div class="d-flex gap-2">
                <button class="btn btn-primary" [disabled]="loading() || f.invalid">
                  @if (loading()) { <span class="spinner-border spinner-border-sm me-2"></span> }
                  <i class="fa-solid fa-floppy-disk me-1"></i>Save
                </button>
                <a class="btn btn-outline-secondary" routerLink="/books">Cancel</a>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class BookForm implements OnInit {
  private bookSvc = inject(BookService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  private id: number | null = null;
  isEdit = signal(false);
  loading = signal(false);
  error = signal('');
  model: BookInput = { title: '', author: '', publishedDate: '' };

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.id = +idParam;
      this.isEdit.set(true);
      this.bookSvc.get(this.id).subscribe((b) => {
        this.model = { title: b.title, author: b.author, publishedDate: b.publishedDate };
      });
    }
  }

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    const done = {
      next: () => this.router.navigate(['/books']),
      error: (err: any) => {
        this.error.set(err?.error ?? 'Save failed.');
        this.loading.set(false);
      },
    };
    if (this.isEdit() && this.id !== null) {
      this.bookSvc.update(this.id, this.model).subscribe(done);
    } else {
      this.bookSvc.create(this.model).subscribe(done);
    }
  }
}
