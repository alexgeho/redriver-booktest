import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Book } from '../models/models';

export type BookInput = Omit<Book, 'id'>;

@Injectable({ providedIn: 'root' })
export class BookService {
  private readonly api = `${environment.apiUrl}/books`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Book[]> {
    return this.http.get<Book[]>(this.api);
  }

  get(id: number): Observable<Book> {
    return this.http.get<Book>(`${this.api}/${id}`);
  }

  create(book: BookInput): Observable<Book> {
    return this.http.post<Book>(this.api, book);
  }

  update(id: number, book: BookInput): Observable<void> {
    return this.http.put<void>(`${this.api}/${id}`, book);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }
}
