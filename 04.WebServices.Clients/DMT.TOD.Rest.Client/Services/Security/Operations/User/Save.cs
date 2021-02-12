﻿#region Usings

using System.Collections.Generic;

#endregion

namespace DMT.Services.Operations
{
    static partial class TOD
    {
        static partial class Security
        {
            static partial class User
            {
                /// <summary>
                /// Save User.
                /// </summary>
                /// <returns>Returns Saved User.</returns>
                public static NRestResult<Models.User> Save(Models.User value)
                {
                    var ret = Execute<Models.User>(
                        RouteConsts.TOD.Security.User.Save.Url, value);
                    return ret;
                }
            }
        }
    }
}