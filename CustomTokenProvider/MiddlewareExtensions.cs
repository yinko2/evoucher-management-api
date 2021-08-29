using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace eVoucherAPI.CustomTokenAuthProvider
{
    #region snippet1
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenProviderMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenProviderMiddleware>();
        }
    }
    #endregion
}
