using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.V1
{
    public static class ServerRoutes
    {
        public const string Root = "connect";
        public const string Version = "v1";
        public const string Base = $"{Root}/{Version}";
        public static class Authorisation
        {
            public const string Authorize = Base + "/authorize";
            public const string Logout = Base + "/logout";
            public const string Token = Base + "/token";
            public const string Introspect = Base + "/introspect";
            public const string Verify = Base + "/verify";
        }
        public static class UserInfo
        {
            public const string Get = Base + "/userinfo";
        }
    }
}
