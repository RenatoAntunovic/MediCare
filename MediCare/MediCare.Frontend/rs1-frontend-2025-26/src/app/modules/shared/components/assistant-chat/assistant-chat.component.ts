import {
  Component,
  DestroyRef,
  ElementRef,
  OnInit,
  afterNextRender,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AssistantApiService } from '../../../../api-services/assistant/assistant-api.service';
import {
  AssistantChatMessage,
  AssistantMedicineSuggestion,
  AssistantSlotSuggestion,
  AssistantTreatmentSuggestion,
} from '../../../../api-services/assistant/assistant-api.models';
import { AssistantSessionService } from './assistant-session.service';

/** Only the last messages are sent – must stay within AssistantChatCommand.MaxMessages on the backend. */
const MAX_HISTORY = 16;
const MAX_LENGTH = 2000;

/**
 * Chat UI for the AI assistant (used by the client and the admin page).
 * Buttons under an answer are emitted as outputs – the page decides what they do
 * (add to cart, open the booking calendar, open the medicine detail).
 */
@Component({
  selector: 'app-assistant-chat',
  standalone: true,
  imports: [FormsModule, MatButtonModule, MatIconModule, MatTooltipModule, TranslateModule],
  templateUrl: './assistant-chat.component.html',
  styleUrl: './assistant-chat.component.scss',
})
export class AssistantChatComponent implements OnInit {
  mode = input<'patient' | 'admin'>('patient');
  /** Translation keys of example questions shown in an empty chat */
  examples = input<string[]>([]);

  addToCart = output<AssistantMedicineSuggestion>();
  openMedicine = output<AssistantMedicineSuggestion>();
  bookSlot = output<AssistantSlotSuggestion>();
  bookTreatment = output<AssistantTreatmentSuggestion>();

  private api = inject(AssistantApiService);
  private translate = inject(TranslateService);
  private destroyRef = inject(DestroyRef);
  session = inject(AssistantSessionService);

  private scroller = viewChild<ElementRef<HTMLElement>>('scroller');
  private inputBox = viewChild<ElementRef<HTMLTextAreaElement>>('inputBox');

  readonly maxLength = MAX_LENGTH;
  draft = '';
  isSending = signal(false);

  constructor() {
    afterNextRender(() => this.scrollToBottom());
  }

  ngOnInit(): void {
    this.session.ensureOwner();
  }

  send(text?: string): void {
    const content = (text ?? this.draft).trim();
    if (!content || this.isSending()) return;

    this.session.add({ role: 'user', content });
    this.draft = '';
    this.isSending.set(true);
    this.scrollToBottom();

    const history: AssistantChatMessage[] = this.session
      .entries()
      .filter(e => !e.isError)
      .slice(-MAX_HISTORY)
      .map(e => ({ role: e.role, content: e.content }));

    this.api
      .chat({ messages: history })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.session.add({ role: 'assistant', content: res.reply, suggestions: res.suggestions });
          this.isSending.set(false);
          this.scrollToBottom();
          this.focusInput();
        },
        error: (err: HttpErrorResponse) => {
          this.session.add({ role: 'assistant', content: this.errorText(err), isError: true });
          this.isSending.set(false);
          this.scrollToBottom();
        },
      });
  }

  sendExample(key: string): void {
    this.send(this.translate.instant(key));
  }

  /** Enter sends, Shift+Enter makes a new line */
  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey && !event.isComposing) {
      event.preventDefault();
      this.send();
    }
  }

  newConversation(): void {
    this.session.clear();
    this.focusInput();
  }

  formatDate(isoDate: string): string {
    const [y, m, d] = isoDate.split('-').map(Number);
    const locale = this.translate.currentLang === 'en' ? 'en-GB' : 'bs-BA';
    return new Date(y, m - 1, d).toLocaleDateString(locale, { weekday: 'short', day: 'numeric', month: 'numeric' });
  }

  formatPrice(value: number): string {
    return 'KM ' + value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  /**
   * Very small, safe markdown: escape everything first, then allow **bold** and "- " lists.
   * The model's text never becomes HTML directly.
   */
  toHtml(text: string): string {
    const escaped = text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');

    const lines = escaped.split(/\r?\n/);
    let html = '';
    let inList = false;

    for (const raw of lines) {
      const line = raw.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
      const item = line.match(/^\s*(?:[-*•]|\d+\.)\s+(.*)$/);

      if (item) {
        if (!inList) { html += '<ul>'; inList = true; }
        html += `<li>${item[1]}</li>`;
        continue;
      }
      if (inList) { html += '</ul>'; inList = false; }
      if (line.trim()) html += `<p>${line}</p>`;
    }
    if (inList) html += '</ul>';
    return html;
  }

  hasSuggestions(s: { medicines: unknown[]; treatments: unknown[]; slots: unknown[] } | undefined): boolean {
    return !!s && (s.medicines.length > 0 || s.treatments.length > 0 || s.slots.length > 0);
  }

  private errorText(err: HttpErrorResponse): string {
    if (err.status === 0) return this.translate.instant('ASSISTANT.ERRORS.OFFLINE');
    if (err.status === 429) return this.translate.instant('ASSISTANT.ERRORS.TOO_MANY');
    // 503 messages from the server are written for users (not configured, Groq limit…)
    if (err.status === 503 && err.error?.message) return err.error.message;
    return this.translate.instant('ASSISTANT.ERRORS.GENERIC');
  }

  private scrollToBottom(): void {
    requestAnimationFrame(() => {
      const el = this.scroller()?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    });
  }

  private focusInput(): void {
    requestAnimationFrame(() => this.inputBox()?.nativeElement.focus());
  }
}
