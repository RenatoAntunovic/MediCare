import {PageRequest} from './page-request';

// mirrors the C# class BasePagedQuery.cs
export class BasePagedQuery {
  paging: PageRequest;

  constructor() {
    this.paging = new PageRequest();
  }
}
