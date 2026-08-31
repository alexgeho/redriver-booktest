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
  // Who added the quote, and whether it belongs to the current user.
  // Everyone sees every quote; only `mine` quotes are editable/deletable.
  ownerUsername?: string | null;
  mine: boolean;
}

export interface AuthResponse {
  token: string;
  username: string;
}
