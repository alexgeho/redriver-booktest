import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { QuoteService, QuoteInput } from '../services/quote.service';
import { Quote } from '../models/models';

@Component({
  selector: 'app-quote-list',
  imports: [FormsModule],
  template: `
    <h1 class="h4 mb-3"><i class="fa-solid fa-quote-right me-2 text-primary"></i>Mina citat</h1>

    @if (loading()) {
      <div class="text-center py-4"><span class="spinner-border text-primary"></span></div>
    } @else {

      <!-- Featured, read-only quotes (visible to everyone, not editable) -->
      <div class="d-flex align-items-center gap-2 mb-2">
        <h2 class="h6 text-body-secondary mb-0">Utvalda citat</h2>
        <span class="badge text-bg-secondary">
          <i class="fa-solid fa-lock me-1"></i>Skrivskyddad
        </span>
      </div>
      <div class="row g-3 mb-4">
        @for (q of featured(); track q.id) {
          <div class="col-12">
            <figure class="card shadow-sm border-0 border-start border-4 border-primary bg-body-tertiary mb-0 h-100">
              <div class="card-body py-3">
                <p class="mb-1 fst-italic">"{{ q.text }}"</p>
                @if (q.author) {
                  <figcaption class="text-body-secondary small mb-0">— {{ q.author }}</figcaption>
                }
              </div>
            </figure>
          </div>
        }
      </div>

      <!-- Add / edit form for the user's own quotes -->
      <div class="card shadow-sm mb-4">
        <div class="card-body p-4">
          <h2 class="h6 text-body-secondary mb-3">{{ editingId() ? 'Edit quote' : 'Add a quote you like' }}</h2>
          <form (ngSubmit)="save()" #f="ngForm">
            <div class="mb-3">
              <label class="form-label">Quote</label>
              <textarea class="form-control" name="text" rows="2" [(ngModel)]="model.text" required></textarea>
            </div>
            <div class="mb-3">
              <label class="form-label">Author</label>
              <input class="form-control" name="author" [(ngModel)]="model.author" placeholder="Optional" />
            </div>
            <div class="d-flex gap-2">
              <button class="btn btn-primary" [disabled]="loading() || f.invalid">
                <i class="fa-solid fa-floppy-disk me-1"></i>{{ editingId() ? 'Update' : 'Add' }}
              </button>
              @if (editingId()) {
                <button type="button" class="btn btn-outline-secondary" (click)="cancelEdit()">Cancel</button>
              }
            </div>
          </form>
        </div>
      </div>

      <!-- The user's own quotes (editable / deletable only by them) -->
      @if (mine().length === 0) {
        <div class="alert alert-info">No quotes of your own yet. Add your favourites above!</div>
      } @else {
        <div class="row g-3">
          @for (q of mine(); track q.id) {
            <div class="col-12">
              <div class="card shadow-sm">
                <div class="card-body d-flex justify-content-between align-items-start">
                  <figure class="mb-0 me-3">
                    <p class="mb-1">"{{ q.text }}"</p>
                    @if (q.author) {
                      <figcaption class="text-body-secondary small mb-0">— {{ q.author }}</figcaption>
                    }
                  </figure>
                  <div class="text-nowrap">
                    <button class="btn btn-sm btn-outline-secondary me-1" (click)="startEdit(q)" title="Edit">
                      <i class="fa-solid fa-pen"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" (click)="remove(q)" title="Delete">
                      <i class="fa-solid fa-trash"></i>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          }
        </div>
      }
    }
  `,
})
export class QuoteList implements OnInit {
  private quoteSvc = inject(QuoteService);

  quotes = signal<Quote[]>([]);
  featured = computed(() => this.quotes().filter((q) => q.isSeed));
  mine = computed(() => this.quotes().filter((q) => q.mine));
  loading = signal(true);
  editingId = signal<number | null>(null);
  model: QuoteInput = { text: '', author: '' };

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.quoteSvc.getAll().subscribe({
      next: (q) => {
        this.quotes.set(q);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  save(): void {
    const id = this.editingId();
    const done = {
      next: () => {
        this.resetForm();
        this.load();
      },
    };
    if (id !== null) {
      this.quoteSvc.update(id, this.model).subscribe(done);
    } else {
      this.quoteSvc.create(this.model).subscribe(done);
    }
  }

  startEdit(q: Quote): void {
    this.editingId.set(q.id);
    this.model = { text: q.text, author: q.author };
  }

  cancelEdit(): void {
    this.resetForm();
  }

  remove(q: Quote): void {
    if (!confirm('Delete this quote?')) return;
    this.quoteSvc.delete(q.id).subscribe(() => this.quotes.update((list) => list.filter((x) => x.id !== q.id)));
  }

  private resetForm(): void {
    this.editingId.set(null);
    this.model = { text: '', author: '' };
  }
}
