// === COMMANDS (WRITE) ===

/**
 * Body for POST /api/Assistant/chat
 * Corresponds to: AssistantChatCommand.cs
 * The whole conversation is sent every time – the server does not store it.
 */
export interface AssistantChatCommand {
  messages: AssistantChatMessage[];
}

export interface AssistantChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

/**
 * Response for POST /api/Assistant/chat
 * Corresponds to: AssistantChatResponseDto.cs
 */
export interface AssistantChatResponseDto {
  reply: string;
  suggestions: AssistantSuggestionsDto;
  /** Admin only: tables with names and notes, built by the server. They were never sent to the AI service. */
  details: AssistantTable[];
}

/** A table attached to an answer (e.g. the reservations with patient names and notes). */
export interface AssistantTable {
  title: string;
  columns: string[];
  rows: string[][];
}

/** Items the assistant recommended (already checked on the server) – shown as buttons. */
export interface AssistantSuggestionsDto {
  medicines: AssistantMedicineSuggestion[];
  treatments: AssistantTreatmentSuggestion[];
  slots: AssistantSlotSuggestion[];
}

export interface AssistantMedicineSuggestion {
  id: number;
  name: string;
  price: number;
  inStock: boolean;
}

export interface AssistantTreatmentSuggestion {
  id: number;
  name: string;
  price: number;
}

export interface AssistantSlotSuggestion {
  treatmentId: number;
  treatmentName: string;
  price: number;
  date: string; // yyyy-MM-dd
  time: string; // HH:mm
}
