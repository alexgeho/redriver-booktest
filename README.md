# BookQuotes — Angular 20 + .NET 9 CRUD with JWT

Test assignment for RedRiver Consulting: a responsive CRUD web app with token-based
authentication. Books CRUD + a per-user **"Mina citat"** page, Bootstrap + Font Awesome
styling, and a light/dark theme toggle.

## Stack
- **Frontend:** Angular 20 (standalone, signals, lazy routes), Bootstrap 5.3, Font Awesome, Bootstrap Icons
- **Backend:** .NET 9 C# Web API, EF Core (SQLite), JWT bearer auth, BCrypt password hashing
- **Repo layout:** `web/` (Angular) · `api/` (.NET API)

## Features
- Register + login; JWT stored in localStorage and attached to every API request via an HTTP interceptor
- Token validation on the backend — all Books/Quotes endpoints require `[Authorize]`
- Books: list, add, edit, delete (redirects back to the list after each action)
- **Mina citat**: per-user favourite quotes — add, edit, delete; menu to switch between Books and Quotes
- Responsive layout (Bootstrap navbar collapses to a mobile menu; tables adapt)
- Light/dark UX toggle (Bootstrap 5.3 `data-bs-theme`)

## Run locally
**API** (http://localhost:5099):
```bash
cd api
dotnet run --urls http://localhost:5099
```
**Frontend** (http://localhost:4200):
```bash
cd web
npm install
npm start
```
The frontend talks to the API via `web/src/environments/environment.ts`.

## Deploy
- **API** → Render (Docker, `api/Dockerfile`). Set env `Jwt__Key` to a long secret. Note: SQLite is ephemeral on free tier.
- **Frontend** → Netlify (`netlify.toml`). Before building, set `apiUrl` in `environment.prod.ts` to the deployed API URL.

## Default seed
Three sample books are seeded on first run. Register a user, then add books/quotes.
