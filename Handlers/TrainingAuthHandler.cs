using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TmsApi.Handlers
{
    /// <summary>
    /// Temporary authentication scheme handler that checks for the presence of the 'X-Training-User' header.
    /// Used for pipeline training purposes to simulate challenges and authenticated states.
    /// </summary>
    public class TrainingAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TrainingAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("X-Training-User"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing training user header."));
            }

            string? trainingUser = Request.Headers["X-Training-User"];
            if (string.IsNullOrWhiteSpace(trainingUser))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid training user header."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, trainingUser)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}