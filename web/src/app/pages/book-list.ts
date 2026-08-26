import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { BookService } from '../services/book.service';
import { Book } from '../models/models';

@Component({
  selector: 'app-book-list',
  imports: [RouterLink, DatePipe],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h1 class="h4 mb-0"><i class="fa-solid fa-book me-2 text-primary"></i>Books</h1>
      <a class="btn btn-primary" routerLink="/books/new">
        <i class="fa-solid fa-plus me-1"></i>Add new book
      </a>
    </div>

    @if (loading()) {
      <div class="text-center py-5"><span class="spinner-border text-primary"></span></div>
    } @else if (books().length === 0) {
      <div class="alert alert-info">No books yet. Add your first one!</div>
    } @else {
      <div class="card shadow-sm">
        <div class="table-responsive">
          <table class="table table-hover align-middle mb-0">
            <thead>
              <tr>
                <th>Title</th>
                <th class="d-none d-sm-table-cell">Author</th>
                <th class="d-none d-md-table-cell">Published</th>
                <th class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (book of books(); track book.id) {
                <tr>
                  <td class="fw-semibold">{{ book.title }}</td>
                  <td class="d-none d-sm-table-cell">{{ book.author }}</td>
                  <td class="d-none d-md-table-cell">
                    {{ book.publishedDate ? (book.publishedDate | date: 'yyyy-MM-dd') : '—' }}
                  </td>
                  <td class="text-end text-nowrap">
                    <a class="btn btn-sm btn-outline-secondary me-1" [routerLink]="['/books', book.id, 'edit']"
                       title="Edit">
                      <i class="fa-solid fa-pen"></i>
                    </a>
                    <button class="btn btn-sm btn-outline-danger" (click)="remove(book)" title="Delete">
                      <i class="fa-solid fa-trash"></i>
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }
  `,
})
export class BookList implements OnInit {
  private bookSvc = inject(BookService);

  books = signal<Book[]>([]);
  loading = signal(true);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.bookSvc.getAll().subscribe({
      next: (b) => {
        this.books.set(b);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  remove(book: Book): void {
    if (!confirm(`Delete "${book.title}"?`)) return;
    this.bookSvc.delete(book.id).subscribe(() => this.books.update((list) => list.filter((b) => b.id !== book.id)));
  }
}
