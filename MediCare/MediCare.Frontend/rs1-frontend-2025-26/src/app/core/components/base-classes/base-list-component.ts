// src/app/core/components/base-list.component.ts

import {BaseComponent} from './base-component';

export abstract class BaseListComponent<TItem> extends BaseComponent{
  items: TItem[] = [];

  /**
   * The concrete data loading is left to the child components.
   */
  protected abstract loadData(): void;

  /**
   * Helper you can call from ngOnInit of a child component.
   */
  protected initList(): void {
    this.loadData();
  }
}
