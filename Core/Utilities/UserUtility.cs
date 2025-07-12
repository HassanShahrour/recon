using System.Security.Claims;

namespace Reconova.Core.Utilities
{
    public class UserUtility
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserUtility(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<Guid> GetLoggedInUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdString = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var parsedUserId = Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;

            return Task.FromResult<Guid>(parsedUserId);
        }

    }
}
