import {PageRequest} from './page-request';

// mirrors the C# class BasePagedQuery.cs
export class BasePagedQuery {
  paging: PageRequest;
  /** Column to sort by on the server (e.g. "name", "price"). */
  sortBy?: string | null;
  /** "asc" | "desc" */
  sortDirection?: 'asc' | 'desc' | null;

  constructor() {
    this.paging = new PageRequest();
  }
}