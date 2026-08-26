import { Injectable, signal } from '@angular/core';

type Theme = 'light' | 'dark';
const THEME_KEY = 'bq_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _theme = signal<Theme>((localStorage.getItem(THEME_KEY) as Theme) || 'light');
  readonly theme = this._theme.asReadonly();

  constructor() {
    this.apply(this._theme());
  }

  toggle(): void {
    this._theme.set(this._theme() === 'light' ? 'dark' : 'light');
    localStorage.setItem(THEME_KEY, this._theme());
    this.apply(this._theme());
  }

  // Bootstrap 5.3 dark mode via the data-bs-theme attribute on <html>.
  private apply(theme: Theme): void {
    document.documentElement.setAttribute('data-bs-theme', theme);
  }
}
