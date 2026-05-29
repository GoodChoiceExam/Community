# Community

Microservice der håndterer den sociale del af FitLife. Medlemmer kan se og skrive opslag i deres centers community-gruppe.

## Hvad servicen kan

- Hente alle communities eller slå et bestemt op pr. center
- Hente opslag i et community
- Oprette et opslag — forfatter og member-id hentes automatisk fra JWT-tokenet

Alle endpoints kræver et gyldigt JWT token udstedt af Identity-servicen.

## RabbitMQ

Lytter på køen `membership.member.created`. Når Membership-servicen opretter et nyt medlem, opretter Community automatisk en community-gruppe til det pågældende center, hvis den ikke allerede eksisterer.

## Struktur

- `Controllers/` — HTTP endpoints
- `Services/` — forretningslogik
- `Repositories/` — MongoDB-adgang
- `Models/` — domæneobjekter (`CenterCommunity`, `CommunityPost`, m.fl.)
- `Messaging/` — RabbitMQ consumer
- `FitLife.Community.Tests/` — unit tests med NUnit