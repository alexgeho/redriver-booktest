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
  // `isSeed` = a featured, read-only quote (shown to everyone, not editable).
  // `mine` = the current user's own quote (the only editable/deletable ones).
  isSeed: boolean;
  mine: boolean;
}

export interface AuthResponse {
  token: string;
  username: string;
}
