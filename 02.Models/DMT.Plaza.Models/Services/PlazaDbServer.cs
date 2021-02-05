//#define USE_PROGRAM_DATA

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.CompilerServices;
using System.Reflection;

// SQLite
using SQLite;
//using SQLiteNetExtensions.Attributes;
//using SQLiteNetExtensions.Extensions;

using NLib;
using NLib.IO;

using DMT.Models;
using DMT.Views;

#endregion

namespace DMT.Services
{
    #region PlazaDbServer

    /// <summary>
    /// Plaza Database Server.
    /// </summary>
    public class PlazaDbServer
    {
        #region Singelton

        private static PlazaDbServer _instance = null;
        /// <summary>
        /// Singelton Access.
        /// </summary>
        public static PlazaDbServer Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (typeof(PlazaDbServer))
                    {
                        _instance = new PlazaDbServer();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Constructor and Destructor

        /// <summary>
        /// Constructor.
        /// </summary>
        private PlazaDbServer() : base()
        {
            this.FileName = "Plaza.db";
        }
        /// <summary>
        /// Destructor.
        /// </summary>
        ~PlazaDbServer()
        {
            Shutdown();
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets local json folder path name.
        /// </summary>
        private static string LocalFolder
        {
            get
            {
#if !USE_PROGRAM_DATA
                string localFilder = Folders.Combine(
                    Folders.Assemblies.CurrentExecutingAssembly, "data");
#else
                // Stored in C:\ProgarmData\DMT\Data\ folder
                string localFilder = ApplicationManager.Instance.Environments.Company.Data.FullName;
#endif
                if (!Folders.Exists(localFilder))
                {
                    Folders.Create(localFilder);
                }
                return localFilder;
            }
        }

        #endregion

        #region Private Methods

        private void InitTables()
        {

        }

        private void InitDefaults()
        {

        }

        private void InitViews()
        {
            if (null == Db) return;

            string prefix;

            // Infrastructures - Embeded resource used . instead / to access sub contents.
            prefix = @"Infrastructures";
            //InitView("PlazaGroupView", 1, prefix);
            //InitView("PlazaView", 1, prefix);
            //InitView("LaneView", 1, prefix);

            // Users - Embeded resource used . instead / to access sub contents.
            prefix = @"Users";
            //InitView("UserView", 1, prefix);

            // Shifts - Embeded resource used . instead / to access sub contents.
            prefix = @"Shifts";
            //InitView("TSBShiftView", 1, prefix);
        }

        class ViewInfo
        {
            public string Name { get; set; }
        }

        private void InitView(string viewName, int version, string resourcePrefix = "")
        {
            if (null == Db) return;

            /*
            var hist = ViewHistory.GetWithChildren(viewName, false).Value();

            string checkViewCmd = "SELECT Name FROM sqlite_master WHERE Type = 'view' AND Name = ?";
            var rets = Db.Query<ViewInfo>(checkViewCmd, viewName);
            bool exists = (null != rets && rets.Count > 0);

            //bool exists = (null != info) ? info.Count > 0 : false;

            if (!exists || null == hist || hist.VersionId < version)
            {
                string script = string.Empty;
                MethodBase med = MethodBase.GetCurrentMethod();
                try
                {
                    string dropCmd = string.Empty;
                    dropCmd += "DROP VIEW IF EXISTS " + viewName;
                    Db.BeginTransaction();
                    try { Db.Execute(dropCmd); }
                    catch (Exception dropEx)
                    {
                        med.Err(dropEx);
                        med.Err("Drop Failed:" + Environment.NewLine + viewName);
                        Db.Rollback();
                    }
                    finally { Db.Commit(); }

                    string resourceName = viewName + ".sql";
                    // Note: 
                    // -----------------------------------------------------------
                    // Embeded resource used . instead / to access sub contents.
                    // -----------------------------------------------------------
                    string embededResourceName;
                    if (!string.IsNullOrWhiteSpace(resourcePrefix))
                    {
                        // Has prefix
                        if (!resourcePrefix.Trim().EndsWith("."))
                        {
                            // Not end with . so append . and concat full name.
                            embededResourceName = @"DMT.Views.Scripts." + resourcePrefix + "." + resourceName;
                        }
                        else
                        {
                            // Already end with . so concat to full name.
                            embededResourceName = @"DMT.Views.Scripts." + resourcePrefix + resourceName;
                        }
                    }
                    else
                    {
                        // No prefix.
                        embededResourceName = @"DMT.Views.Scripts." + resourceName;
                    }

                    script = PlazaSqliteScriptManager.GetScript(embededResourceName);

                    if (!string.IsNullOrEmpty(script))
                    {
                        var ret = Db.Execute(script);
                        //Console.WriteLine("Returns: {0}", ret);

                        if (null == hist) hist = new ViewHistory();
                        hist.ViewName = viewName;
                        hist.VersionId = version;
                        ViewHistory.Save(hist);

                        string msg = string.Format("Update View {0}, version {1}.", hist.ViewName, hist.VersionId);
                        Console.WriteLine(msg);
                        med.Info(msg);
                    }
                    else
                    {
                        Console.WriteLine("{0} Has Empty Scripts.", viewName);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                    med.Err(ex);
                    med.Err("Script:" + Environment.NewLine + script);
                    //Console.WriteLine(script);
                }
            }
            */
        }

        #endregion

        #region Public Methods (Start/Shutdown)

        /// <summary>
        /// Start.
        /// </summary>
        public void Start()
        {
            MethodBase med = MethodBase.GetCurrentMethod();
            if (null == Db)
            {
                lock (typeof(PlazaDbServer))
                {
                    try
                    {
                        // ---------------------------------------------------------------
                        // NOTE:
                        // ---------------------------------------------------------------
                        // If Exception due to version mismatch here
                        // Please rebuild only this project and try again
                        // VS Should Solve mismatch version properly (maybe)
                        // See: https://nickcraver.com/blog/2020/02/11/binding-redirects/
                        // for more information.
                        // ---------------------------------------------------------------

                        string path = Path.Combine(LocalFolder, FileName);
                        Db = new SQLiteConnection(path,
                            SQLiteOpenFlags.Create |
                            SQLiteOpenFlags.SharedCache |
                            SQLiteOpenFlags.ReadWrite |
                            SQLiteOpenFlags.FullMutex,
                            storeDateTimeAsTicks: false);
                        Db.BusyTimeout = new TimeSpan(0, 0, 5); // set busy timeout.
                    }
                    catch (Exception ex)
                    {
                        med.Err(ex);
                        Db = null;

                        OnConectError.Call(this, EventArgs.Empty);
                    }
                    if (null != Db)
                    {
                        // Set Default connection 
                        // (be careful to make sure that we only has single database
                        // for all domain otherwise call static method with user connnection
                        // in each domain class instead omit connection version).

                        NTable.Default = Db;
                        NQuery.Default = Db;
                        InitTables(); // Init Tables.
                        InitDefaults(); // init default data.
                        InitViews(); // init views.

                        OnConnected.Call(this, EventArgs.Empty);
                    }
                }
            }
        }
        /// <summary>
        /// Shutdown.
        /// </summary>
        public void Shutdown()
        {
            if (null != Db)
            {
                Db.Dispose();
            }
            Db = null;
            OnDisconnected.Call(this, EventArgs.Empty);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets database file name.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Gets SQLite Connection.
        /// </summary>
        public SQLiteConnection Db { get; private set; }
        /// <summary>
        /// Gets is database connected.
        /// </summary>
        public bool Connected { get { return (null != this.Db); } }

        #endregion

        #region Public Events

        /// <summary>
        /// OnConnected event.
        /// </summary>
        public event EventHandler OnConnected;
        /// <summary>
        /// OnDisconnected event.
        /// </summary>
        public event EventHandler OnDisconnected;
        /// <summary>
        /// OnConectError event.
        /// </summary>
        public event EventHandler OnConectError;

        #endregion
    }

    #endregion
}
