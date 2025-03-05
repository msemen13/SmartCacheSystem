using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SmartCache.Client.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey))
                return Task.FromResult(AuthenticateResult.Fail("Missing API Key"));

            var configuredApiKey = _configuration["ApiKey"];
            if (string.IsNullOrEmpty(configuredApiKey) || extractedApiKey != configuredApiKey)
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));

            var claims = new[] { new Claim(ClaimTypes.Name, "API User") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));        }
    }
}