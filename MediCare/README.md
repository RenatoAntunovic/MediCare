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
