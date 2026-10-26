import { Injectable, inject, signal } from '@angular/core';
import { AuthFacadeService } from '../../../../core/services/auth/auth-facade.service';
import { AssistantSuggestionsDto } from '../../../../api-services/assistant/assistant-api.models';

export interface AssistantChatEntry {
  role: 'user' | 'assistant';
  content: string;
  suggestions?: AssistantSuggestionsDto;
  /** A failed request – shown, but never sent back to the model */
  isError?: boolean;
}

/**
 * Keeps the conversation in memory only (no localStorage, nothing on the server):
 * it survives navigating between pages, and is gone after a reload or when another user logs in.
 */
@Injectable({ providedIn: 'root' })
export class AssistantSessionService {
  private auth = inject(AuthFacadeService);
  private ownerId: number | null = null;

  readonly entries = signal<AssistantChatEntry[]>([]);

  /** Call when the chat opens – clears a conversation that belongs to a different user. */
  ensureOwner(): void {
    const userId = this.auth.currentUser()?.userId ?? null;
    if (userId !== this.ownerId) {
      this.ownerId = userId;
      this.entries.set([]);
    }
  }

  add(entry: AssistantChatEntry): void {
    this.entries.update(list => [...list, entry]);
  }

  clear(): void {
    this.entries.set([]);
  }
}
