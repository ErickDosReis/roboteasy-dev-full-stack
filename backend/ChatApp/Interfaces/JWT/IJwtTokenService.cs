using ChatApp.Models;

namespace ChatApp.Interfaces.JWT
{
    public interface IJwtTokenService
    {
        string Generate(ApplicationUser user, IList<string> roles);
    }

}
