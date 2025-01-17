using System.Collections.Generic;

namespace JWTAPI.Controllers.Resources
{
    public class UserResource
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}