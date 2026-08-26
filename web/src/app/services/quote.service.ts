import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Quote } from '../models/models';

export type QuoteInput = Pick<Quote, 'text' | 'author'>;

@Injectable({ providedIn: 'root' })
export class QuoteService {
  private readonly api = `${environment.apiUrl}/quotes`;

  constructor(private http: HttpClient) {}

  getMine(): Observable<Quote[]> {
    return this.http.get<Quote[]>(this.api);
  }

  create(quote: QuoteInput): Observable<Quote> {
    return this.http.post<Quote>(this.api, quote);
  }

  update(id: number, quote: QuoteInput): Observable<void> {
    return this.http.put<void>(`${this.api}/${id}`, quote);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }
}
