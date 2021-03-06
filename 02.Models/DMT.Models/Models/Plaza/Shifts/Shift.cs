﻿#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using NLib;
using NLib.Design;
using NLib.Reflection;

using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
// required for JsonIgnore attribute.
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Reflection;

#endregion

namespace DMT.Models
{
    #region Shift

    /// <summary>
    /// The Shift Data Model class.
    /// </summary>
    [TypeConverter(typeof(PropertySorterSupportExpandableTypeConverter))]
    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    //[Table("Shift")]
    public class Shift : NTable<Shift>
    {
        #region Intenral Variables

        private int _ShiftId = 0;
        private string _ShiftNameTH = string.Empty;
        private string _ShiftNameEN = string.Empty;

        private DateTime? _TimeStart = new DateTime?();
        private DateTime? _TimeEnd = new DateTime?();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public Shift() : base() { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check is current datetime is between current shift.
        /// </summary>
        /// <returns></returns>
        public bool CheckIsCurrent()
        {
            MethodBase med = MethodBase.GetCurrentMethod();

            DateTime now = DateTime.Now;
            DateTime dt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);

            // Test 2021-02-12 01:xx:xx
            //DateTime dt = new DateTime(now.Year, now.Month, 12, 01, now.Minute, now.Second, now.Millisecond);
            // Test 2021-02-12 22:xx:xx
            //DateTime dt = new DateTime(now.Year, now.Month, 12, 22, now.Minute, now.Second, now.Millisecond);

            if (!TimeStart.HasValue || !TimeEnd.HasValue)
            {
                return false; // No data found.
            }

            DateTime timeStart = TimeStart.Value;
            DateTime timeEnd = TimeEnd.Value;

            if (timeStart.Hour == timeEnd.Hour &&
                timeStart.Minute == timeEnd.Minute &&
                timeStart.Second == timeEnd.Second)
            {
                return false; // Same Hour/Minute/Second
            }
            DateTime start = new DateTime(dt.Year, dt.Month, dt.Day,
                timeStart.Hour, timeStart.Minute, timeStart.Second, timeStart.Millisecond);

            DateTime end = new DateTime(dt.Year, dt.Month, dt.Day,
                timeEnd.Hour, timeEnd.Minute, timeEnd.Second, timeEnd.Millisecond);

            if (TimeStart.Value.Hour > TimeEnd.Value.Hour)
            {
                if (dt.Hour < TimeStart.Value.Hour)
                {
                    // If current time less than TimeStart that mean its yesterday so substract day.
                    start = start.AddDays(-1);
                }
                else
                {
                    // Condition - Shift cover between 2 days (normaly shift - 3).
                    end = end.AddDays(1);
                }
            }

            bool ret = (dt >= start && dt < end);

            if (ret)
            {
                string msg = string.Format(
                    "Match Shift {0}, Start: {1:dd/MM/yyyy HH:mm} - End: {2:dd/MM/yyyy HH:mm}, Curr: {3:dd/MM/yyyy HH:mm}",
                    ShiftId, start, end, dt);
                Console.WriteLine(msg);
                med.Info(msg);
            }
            return ret;
        }

        #endregion

        #region Public Proprties

        #region Common

        /// <summary>
        /// Gets or sets ShiftId.
        /// </summary>
        [Category("Common")]
        [Description("Gets or sets ShiftId.")]
        [PrimaryKey]
        [PropertyMapName("ShiftId")]
        public int ShiftId
        {
            get
            {
                return _ShiftId;
            }
            set
            {
                if (_ShiftId != value)
                {
                    _ShiftId = value;
                    this.RaiseChanged("ShiftId");
                }
            }
        }
        /// <summary>
        /// Gets or sets Name TH.
        /// </summary>
        [Category("Common")]
        [Description("Gets or sets Name TH.")]
        [MaxLength(50)]
        [PropertyMapName("ShiftNameTH")]
        public string ShiftNameTH
        {
            get
            {
                return _ShiftNameTH;
            }
            set
            {
                if (_ShiftNameTH != value)
                {
                    _ShiftNameTH = value;
                    this.RaiseChanged("ShiftNameTH");
                }
            }
        }
        /// <summary>
        /// Gets or sets Name EN.
        /// </summary>
        [Category("Common")]
        [Description("Gets or sets Name EN.")]
        [MaxLength(50)]
        [PropertyMapName("ShiftNameEN")]
        public string ShiftNameEN
        {
            get
            {
                return _ShiftNameEN;
            }
            set
            {
                if (_ShiftNameEN != value)
                {
                    _ShiftNameEN = value;
                    this.RaiseChanged("ShiftNameEN");
                }
            }
        }
        /// <summary>
        /// Gets or sets Time Start.
        /// </summary>
        [Category("Common")]
        [Description("Gets or sets Time Start.")]
        [PropertyMapName("TimeStart")]
        public DateTime? TimeStart
        {
            get
            {
                return _TimeStart;
            }
            set
            {
                if (_TimeStart != value)
                {
                    _TimeStart = value;
                    this.RaiseChanged("TimeStart");
                }
            }
        }
        /// <summary>
        /// Gets or sets Time End.
        /// </summary>
        [Category("Common")]
        [Description("Gets or sets TimeEnd.")]
        [PropertyMapName("TimeEnd")]
        public DateTime? TimeEnd
        {
            get
            {
                return _TimeEnd;
            }
            set
            {
                if (_TimeEnd != value)
                {
                    _TimeEnd = value;
                    this.RaiseChanged("TimeEnd");
                }
            }
        }

        #endregion

        #endregion

        #region Static Methods

        #region Get Shifts

        /// <summary>
        /// Gets Shifts.
        /// </summary>
        /// <param name="db">The database connection.</param>
        /// <returns>Returns List of Shift.</returns>
        public static NDbResult<List<Shift>> GetShifts(SQLiteConnection db)
        {
            var result = new NDbResult<List<Shift>>();
            if (null == db)
            {
                result.DbConenctFailed();
                return result;
            }
            lock (sync)
            {
                MethodBase med = MethodBase.GetCurrentMethod();
                try
                {
                    string cmd = string.Empty;
                    cmd += "SELECT * FROM Shift ";
                    var data = NQuery.Query<Shift>(cmd);
                    if (null != data)
                    {
                        // Note: Auto create TimeStart/TimeEnd of shift when no value assigned.
                        data.ForEach(shift => 
                        {
                            bool needSave = false;
                            if (!shift.TimeStart.HasValue)
                            {
                                if (shift.ShiftId == 1)
                                {
                                    shift.TimeStart = new DateTime(1, 1, 1, 5, 0, 0, 0, 0);
                                }
                                else if (shift.ShiftId == 2)
                                {
                                    shift.TimeStart = new DateTime(1, 1, 1, 13, 0, 0, 0, 0);
                                }
                                else if (shift.ShiftId == 3)
                                {
                                    shift.TimeStart = new DateTime(1, 1, 1, 21, 0, 0, 0, 0);
                                }
                                else
                                {
                                    shift.TimeStart = new DateTime(1, 1, 1, 0, 0, 0, 0, 0);
                                }
                                needSave = true;
                            }
                            if (!shift.TimeEnd.HasValue)
                            {
                                if (shift.ShiftId == 1)
                                {
                                    shift.TimeEnd = new DateTime(1, 1, 1, 13, 0, 0, 0, 0);
                                }
                                else if (shift.ShiftId == 2)
                                {
                                    shift.TimeEnd = new DateTime(1, 1, 1, 21, 0, 0, 0, 0);
                                }
                                else if (shift.ShiftId == 3)
                                {
                                    shift.TimeEnd = new DateTime(1, 1, 1, 5, 0, 0, 0, 0);
                                }
                                else
                                {
                                    shift.TimeEnd = new DateTime(1, 1, 1, 0, 0, 0, 0, 0);
                                }
                                needSave = true;
                            }
                            if (needSave)
                            {
                                //Console.WriteLine("Shift: {0} update. Start: {1}, End: {2} ",
                                //    shift.ShiftId, 
                                //    (shift.TimeStart.HasValue) ? shift.TimeStart.Value.ToString("HH:mm:ss") : "00:00:00",
                                //    (shift.TimeEnd.HasValue) ? shift.TimeEnd.Value.ToString("HH:mm:ss") : "00:00:00");
                                Save(shift);
                            }
                        });
                    }
                    result.Success(data);
                }
                catch (Exception ex)
                {
                    med.Err(ex);
                    result.Error(ex);
                }
                return result;
            }
        }
        /// <summary>
        /// Gets Shifts.
        /// </summary>
        /// <returns>Returns List of Shift.</returns>
        public static NDbResult<List<Shift>> GetShifts()
        {
            lock (sync)
            {
                SQLiteConnection db = Default;
                return GetShifts(db);
            }
        }

        #endregion

        #region Save Shift

        /// <summary>
        /// Save Shift.
        /// </summary>
        /// <param name="value">The Shift instance.</param>
        /// <returns>Returns Shift instance.</returns>
        public static NDbResult<Shift> SaveShift(Shift value)
        {
            var result = new NDbResult<Shift>();
            SQLiteConnection db = Default;
            if (null == db)
            {
                result.DbConenctFailed();
                return result;
            }
            lock (sync)
            {
                MethodBase med = MethodBase.GetCurrentMethod();
                try
                {
                    result = Save(value);

                }
                catch (Exception ex)
                {
                    med.Err(ex);
                    result.Error(ex);
                }
                return result;
            }
        }

        #endregion

        #endregion
    }

    #endregion
}
