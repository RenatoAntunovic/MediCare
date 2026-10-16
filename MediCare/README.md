# rs1-template-project

## Demo podaci

Kad pokrenete API u Development okruženju, u bazu se automatski dodaju demo pacijenti, narudžbe i
rezervacije za zadnjih 12 mjeseci (plus nekoliko budućih termina), dodatni lijekovi, tretmani i zalihe –
za prezentaciju i stranicu **Statistika**.

```bash
cd MediCare.Backend/MediCare.API
dotnet run
```

- Demo podaci se dodaju samo jednom; na sljedećim pokretanjima se preskaču.
- U Production okruženju se nikad ne dodaju.

| Nalog | Email | Lozinka |
|---|---|---|
| Admin | `admin@medicare.com` | `admin123!` |
| Admin (osnovni seed) | `admin@market.com` | `Admin` |
| Korisnik (osnovni seed) | `client@gmail.com` | `test123` |
| Demo pacijenti (12) | npr. `amra.hodzic@demo.medicare.com` | `demo123!` |

## AI asistent (Groq)

Stranica **AI asistent** (pacijent i admin) koristi [Groq](https://console.groq.com) – besplatni plan je dovoljan.
Svako postavi svoj API ključ lokalno (user-secrets, ne commituje se):

```bash
cd MediCare.Backend/MediCare.API
dotnet user-secrets set "Groq:ApiKey" "gsk_..."
```

- Model se mijenja preko `Groq:Model` (zadano `openai/gpt-oss-120b`).
- Bez ključa aplikacija radi normalno, samo asistent javlja da nije podešen.
- Asistent samo čita podatke i predlaže; rezervaciju i dodavanje u korpu korisnik potvrđuje sam.
