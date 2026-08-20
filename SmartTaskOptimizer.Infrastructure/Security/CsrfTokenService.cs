using System.Security.Cryptography;

namespace SmartTaskOptimizer.Infrastructure.Security
{
    public sealed class CsrfTokenService
    {
        public const string CookieName = "smarttask.csrf";
        public const string HeaderName = "X-CSRF-TOKEN";

        public string GenerateToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32));
        }

        public bool Validate(
            string? cookieToken,
            string? headerToken)
        {
            if (string.IsNullOrWhiteSpace(cookieToken) ||
                string.IsNullOrWhiteSpace(headerToken))
            {
                return false;
            }

            try
            {
                var cookieBytes =
                    Convert.FromBase64String(cookieToken);

                var headerBytes =
                    Convert.FromBase64String(headerToken);

                return CryptographicOperations.FixedTimeEquals(
                    cookieBytes,
                    headerBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
