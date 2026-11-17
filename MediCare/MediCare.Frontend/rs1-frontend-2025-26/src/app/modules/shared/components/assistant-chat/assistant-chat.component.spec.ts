import { TestBed } from '@angular/core/testing';
import { TranslateService } from '@ngx-translate/core';
import { of, Subject } from 'rxjs';
import { AssistantChatComponent } from './assistant-chat.component';
import { AssistantApiService } from '../../../../api-services/assistant/assistant-api.service';
import { AuthFacadeService } from '../../../../core/services/auth/auth-facade.service';

describe('AssistantChatComponent (admin tables)', () => {
  // The server answers with a short text for the model's side and a table with names for the screen
  const response = {
    reply: 'Imate 1 rezervaciju u 10:30.',
    suggestions: { medicines: [], treatments: [], slots: [] },
    details: [
      { title: 'Rezervacije', columns: ['Pacijent', 'Napomena'], rows: [['Amra Hodžić', 'Dijabetes tip 2']] }
    ]
  };

  let chat: { chat: ReturnType<typeof vi.fn> };
  let component: AssistantChatComponent;

  beforeEach(() => {
    chat = { chat: vi.fn().mockReturnValue(of(response)) };

    TestBed.configureTestingModule({
      providers: [
        { provide: AssistantApiService, useValue: chat },
        { provide: AuthFacadeService, useValue: { currentUser: () => ({ userId: 1 }) } },
        { provide: TranslateService, useValue: { currentLang: 'bs', onLangChange: new Subject(), instant: (k: string) => k } }
      ]
    }).overrideComponent(AssistantChatComponent, { set: { template: '', imports: [] } });

    component = TestBed.createComponent(AssistantChatComponent).componentInstance;
    component.ngOnInit();
  });

  it('keeps the table with names next to the answer, for the screen', () => {
    component.send('Ko je rezervisan?');

    const answer = component.session.entries().at(-1)!;
    expect(answer.role).toBe('assistant');
    expect(answer.details?.[0].rows[0]).toEqual(['Amra Hodžić', 'Dijabetes tip 2']);
  });

  it('never sends the table (names, notes) back to the server as chat history', () => {
    component.send('Ko je rezervisan?');
    component.send('A koliko ih je ukupno?');

    // second request: question, earlier answer (text only), new question
    const history = chat.chat.mock.calls[1][0].messages;
    expect(history.map((m: { role: string }) => m.role)).toEqual(['user', 'assistant', 'user']);
    expect(history[1].content).toBe('Imate 1 rezervaciju u 10:30.');
    const sent = JSON.stringify(history);
    expect(sent).not.toContain('Amra');
    expect(sent).not.toContain('Dijabetes');
  });
});
