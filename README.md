# FitLife Community

Community is an ASP.NET Core Web API microservice for local center communities and posts.

Community does not issue JWT tokens. Identity owns login and token issuing. Community only validates JWT Bearer tokens from Identity through the JWKS endpoint.

## Run Locally

Start MongoDB and Identity first. For local development, the default configuration expects:

```text
mongodb://localhost:27017
http://localhost:5244/.well-known/jwks.json
```

From the repository root:

```powershell
dotnet restore FitLife.Community.slnx
dotnet run --project FitLife.Community.Api/FitLife.Community.Api.csproj
```

Swagger is available here:

```text
http://localhost:5246/swagger
```

Health check:

```text
GET http://localhost:5246/healthz
```

Protected endpoints require a JWT token from Identity:

```text
Authorization: Bearer <jwtToken>
```

## Endpoints

```text
GET  /api/communities
GET  /api/communities/{id}
POST /api/communities
GET  /api/communities/{id}/posts
POST /api/communities/{id}/posts
GET  /api/community/activity/recent
```

`/healthz`, `/swagger`, community reads, post reads, and recent activity are public. Creating communities and posts requires JWT authentication.
