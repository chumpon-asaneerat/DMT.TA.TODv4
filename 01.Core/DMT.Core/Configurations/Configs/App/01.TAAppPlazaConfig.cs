﻿#region Using

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using NLib;
using NLib.IO;
using Newtonsoft.Json;
using NLib.Controls.Design;

#endregion

namespace DMT.Configurations
{
    #region TAAppPlazaConfig (Combine configuration used in TA Plaza applicaltion)

    /// <summary>
    /// The TAAppPlazaConfig class.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class TAAppPlazaConfig
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public TAAppPlazaConfig() : base()
        {
            this.DMT = new DMTConfig();
            this.SCW = new SCWWebServiceConfig();
            this.RabbitMQ = new RabbitMQServiceConfig();
            this.TAxTOD = new TAxTODWebServiceConfig();
            this.TAApp = new TAAppWebServiceConfig();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// IsEquals.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IsEquals(object obj)
        {
            if (null == obj || !(obj is TAAppPlazaConfig)) return false;
            return this.GetString() == (obj as TAAppPlazaConfig).GetString();
        }
        /// <summary>
        /// GetString.
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            string code = string.Empty;
            // Application
            if (null == this.DMT)
            {
                code += "DMT: null" + Environment.NewLine;
            }
            else
            {
                code += string.Format("DMT: {0}",
                    this.DMT.GetString()) + Environment.NewLine;
            }
            // Local
            if (null == this.RabbitMQ)
            {
                code += "RabbitMQ null" + Environment.NewLine;
            }
            else
            {
                code += string.Format("RabbitMQ: {0}",
                    this.RabbitMQ.GetString()) + Environment.NewLine;
            }
            // SCW server
            if (null == this.SCW)
            {
                code += "SCW: null" + Environment.NewLine;
            }
            else
            {
                code += string.Format("SCW: {0}",
                    this.SCW.GetString()) + Environment.NewLine;
            }
            // TAxTOD Server
            if (null == this.TAxTOD)
            {
                code += "TAxTOD: null" + Environment.NewLine;
            }
            else
            {
                code += string.Format("TAxTOD: {0}",
                    this.TAxTOD.GetString()) + Environment.NewLine;
            }
            // TA Application (Plaza)
            if (null == this.TAApp)
            {
                code += "TAApp: null" + Environment.NewLine;
            }
            else
            {
                code += string.Format("TAApp: {0}",
                    this.TAApp.GetString()) + Environment.NewLine;
            }
            return code;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets DMT Config.
        /// </summary>
        public DMTConfig DMT { get; set; }
        /// <summary>
        /// Gets or sets Rabbit MQ Service Config.
        /// </summary>
        public RabbitMQServiceConfig RabbitMQ { get; set; }
        /// <summary>
        /// Gets or sets SCW Service Config.
        /// </summary>
        public SCWWebServiceConfig SCW { get; set; }
        /// <summary>
        /// Gets or sets TAxTOD Service Config.
        /// </summary>
        public TAxTODWebServiceConfig TAxTOD { get; set; }
        /// <summary>
        /// Gets or sets TA App Service Config (local server).
        /// </summary>
        public TAAppWebServiceConfig TAApp { get; set; }

        #endregion
    }

    #endregion
}
