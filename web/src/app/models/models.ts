export interface Book {
  id: number;
  title: string;
  author: string;
  publishedDate: string;
}

export interface Quote {
  id: number;
  text: string;
  author: string;
}

export interface AuthResponse {
  token: string;
  username: string;
}
