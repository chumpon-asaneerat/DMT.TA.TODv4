﻿#region Usings

using System.Collections.Generic;

#endregion

namespace DMT.Services.Operations
{
    static partial class TA
    {
        static partial class Security
        {
            static partial class Role
            {
                /// <summary>
                /// Save Role.
                /// </summary>
                /// <returns>Returns Saved Role.</returns>
                public static NRestResult<Models.Role> Save(Models.Role value)
                {
                    var ret = Execute<Models.Role>(
                        RouteConsts.TA.Security.Role.Save.Url, value);
                    return ret;
                }
            }
        }
    }
}
