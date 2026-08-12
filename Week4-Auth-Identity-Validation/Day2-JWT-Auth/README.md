# Day 2 — JWT Authentication & Token Issuance

**8 hours**

## Learning Objectives

- Explain a JWT's structure and what claims represent
- Implement a login endpoint that issues a JWT on successful authentication
- Configure JWT bearer authentication middleware to validate incoming tokens

## What I Did

- Added `Microsoft.AspNetCore.Authentication.JwtBearer` and implemented
  `POST /api/v1/auth/login`, verifying credentials with
  `SignInManager.CheckPasswordSignInAsync` and returning a generic
  "Invalid email or password" message on failure (401) — deliberately
  not revealing whether the email itself exists
- On successful login, built and signed a JWT containing `sub` and
  `email` claims using `JwtRegisteredClaimNames` (standard short claim
  names instead of the long `ClaimTypes` URIs .NET uses by default)
- Configured JWT bearer authentication in `Program.cs` with issuer,
  audience, and a signing key stored in the gitignored
  `appsettings.Development.json`
- Added a protected test endpoint (`TestController`) decorated with
  `[Authorize]` to verify the full authentication flow end-to-end
- Decoded the issued token at jwt.io and confirmed all claims were
  correct
- Set a short expiry (initially 20 seconds, then reverted to 15
  minutes) and confirmed a token is rejected once expired
- Added Swagger's "Authorize" button (via `AddSecurityDefinition` /
  `AddSecurityRequirement`) so bearer tokens can be tested directly
  from Swagger UI, not just Postman

## Key Code Example

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    var user = await _userManager.FindByEmailAsync(request.Email);
    if (user == null)
        return Unauthorized(new { message = "Invalid email or password." });

    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
    if (!result.Succeeded)
        return Unauthorized(new { message = "Invalid email or password." });

    var token = GenerateJwtToken(user);
    return Ok(new { token });
}
```

## What I Learned — Debugging a Real Authentication Conflict

This day involved the most involved debugging of the program so far,
and every step taught something concrete:

**1. Identity's default cookie auth silently overrides JWT.** Even
after registering `AddJwtBearer`, the protected endpoint kept
redirecting to `/Account/Login` instead of returning 401 — a dead
giveaway of cookie-based auth, not JWT. The fix was setting
`DefaultAuthenticateScheme` and `DefaultChallengeScheme` explicitly to
JWT in `AddAuthentication()`, since `AddIdentity` registers its own
cookie scheme that wins by default unless told otherwise.

**2. Diagnostic logging turned an opaque 401 into a readable error.**
Adding `OnAuthenticationFailed` and `OnChallenge` handlers to log the
exact rejection reason (`IDX10223: token expired`, `IDX10517:
signature validation failed`) made debugging deterministic instead of
guesswork — a blank 401 gives zero information about *why*.

**3. Package version conflicts can break working code with no code
changes.** Installing Swashbuckle for Swagger's "Authorize" button
pulled in `Microsoft.OpenApi 2.0.0` implicitly, which restructured
`OpenApiSecurityScheme`/`OpenApiReference` entirely — code that
compiled fine against the classic API stopped compiling with cryptic
`CS0234`/`CS0117` errors. The actual root cause was
`Microsoft.AspNetCore.OpenApi` (the newer, built-in .NET OpenAPI
system) coexisting with Swashbuckle and forcing the newer
`Microsoft.OpenApi` version — removing the unused package resolved
the conflict without touching the security configuration code at all.

**4. A stale test token isn't the same bug as a broken endpoint.**
Once Swagger confirmed the full flow worked end-to-end, Postman still
failing with 401 was correctly diagnosed as an expired token cached
in a collection variable from an earlier run — not a regression in
the API itself. Distinguishing "the system is broken" from "my test
data is stale" was itself a useful debugging lesson.

## Project

[`TaskTrackerApi/`](TaskTrackerApi/) — JWT login endpoint, bearer
authentication middleware, and a protected test endpoint, verified
via Swagger and jwt.io.