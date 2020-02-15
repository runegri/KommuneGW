// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer
{
    public static class Constants
    {
        public static class AuthenticationSchemes
        {
            public const string StavangerKommune = "stavanger";
            public const string BergenKommune = "bergen";
            public const string IdPorten = "idporten";
            public const string Google = "google";
        }

        public static class ClaimTypes
        {
            public const string Pid = "pid";
            public const string PreferredUserName = "preferred_username";
            public const string UniqueName = "unique_name";            
            public const string Phone = "Phone";            
        }
    }
}
