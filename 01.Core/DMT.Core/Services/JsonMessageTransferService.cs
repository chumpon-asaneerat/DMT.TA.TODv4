﻿#region Using

using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Timers;
using System.Threading.Tasks;

using NLib;
using NLib.IO;

#endregion

namespace DMT.Services
{
    /// <summary>
    /// The Json Message Transfer Service class.
    /// </summary>
    public abstract class JsonMessageTransferService
    {
        #region Constructor and Destructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public JsonMessageTransferService() : base() { }
        /// <summary>
        /// Destructor.
        /// </summary>
        ~JsonMessageTransferService() 
        {
            this.Shutdown();
        }

        #endregion

        #region Internal Variables

        private Timer _timer = null;
        private bool _scanning = false;

        #endregion

        #region Private Methods

        #region Timer Handlers

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_scanning) return;
            _scanning = true;

            MethodBase med = MethodBase.GetCurrentMethod();
            try
            {
                CompressFiles();
                List<string> files = new List<string>();
                var msgFiles = Directory.GetFiles(this.MessageFolder, "*.json");
                if (null != msgFiles && msgFiles.Length > 0) files.AddRange(msgFiles);
                files.ForEach(file =>
                {
                    try
                    {
                        string json = ReadAllText(file);
                        ProcessJson(file, json);
                    }
                    catch (Exception ex2)
                    {
                        // Invalid. Read file error.
                        MoveToInvalid(file);
                        med.Err(ex2);
                    }
                });
            }
            catch (Exception ex)
            {
                med.Err(ex);
            }
            _scanning = false;
        }

        #endregion

        #endregion

        #region Protected Methods and Properties

        #region FolderName property

        /// <summary>
        /// Gets Folder Name (sub directory name).
        /// </summary>
        protected abstract string FolderName { get; }

        #endregion

        #region File Read

        /// <summary>
        /// Read All Text from target file.
        /// </summary>
        /// <param name="fileName">The target full file name.</param>
        /// <returns>Returns text in target file.</returns>
        protected virtual string ReadAllText(string fileName)
        {
            string text = string.Empty;

            MethodBase med = MethodBase.GetCurrentMethod();
            FileStream fs = null;

            #region Open File Steram (for read)

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                med.Err(ex);
                text = string.Empty;
            }

            #endregion

            #region Read File Content

            try
            {
                if (null != fs)
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        text = reader.ReadToEnd();
                        reader.Close();
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex2)
            {
                med.Err(ex2);
                text = string.Empty;
            }

            #endregion

            #region Close File Steram

            if (null != fs)
            {
                try
                {
                    fs.Close();
                    fs.Dispose();
                }
                catch { }
            }
            fs = null;

            #endregion

            return text;
        }

        #endregion

        #region GetFileName

        /// <summary>
        /// Get File Name.
        /// </summary>
        /// <param name="msgType">The message type.</param>
        /// <returns>Returns file name</returns>
        protected string GetFileName(string msgType)
        {

            if (string.IsNullOrWhiteSpace(msgType))
                return string.Empty;
            // Save message.
            string fileName = "msg." + DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss.ffffff",
                System.Globalization.DateTimeFormatInfo.InvariantInfo) + "." + msgType;
            return fileName;
        }

        #endregion

        #region ProcessJson

        /// <summary>
        /// Process Json (string).
        /// </summary>
        /// <param name="fullFileName">The json full file name.</param>
        /// <param name="jsonString">The json data in string.</param>
        protected abstract void ProcessJson(string fullFileName, string jsonString);

        #endregion

        #region Compress file

        /// <summary>
        /// Compress Files.
        /// </summary>
        protected void CompressFiles()
        {
            MethodBase med = MethodBase.GetCurrentMethod();

            List<string> files = new List<string>();
            List<string> oldFiles = new List<string>();

            string backupFolder = Path.Combine(this.MessageFolder, "Backup");
            if (!Directory.Exists(backupFolder)) return; // No Backup Folder.

            var backupFiles = Directory.GetFiles(backupFolder, "*.json");
            if (null != backupFiles && backupFiles.Length > 0) files.AddRange(backupFiles);

            // Find old files to compress.
            DateTime today = DateTime.Today;
            DateTime firstOnMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime targetDT = firstOnMonth.AddMonths(-2); // last 2 month.

            files.ForEach(file => 
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    string fileYMD = fileName.Substring(4, 10);
                    DateTime fileDT;
                    if (DateTime.TryParseExact(fileYMD, "yyyy.MM.dd",
                        System.Globalization.DateTimeFormatInfo.InvariantInfo,
                        System.Globalization.DateTimeStyles.None,
                        out fileDT))
                    {
                        if (fileDT < targetDT)
                        {
                            oldFiles.Add(file);
                        }
                    }
                }
                catch (Exception ex1)
                {
                    med.Err(ex1);
                }
            });
            // Move old files to target folder.
            if (null != oldFiles && oldFiles.Count > 0)
            {
                string zipDir = targetDT.ToString("yyyy.MM.dd", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                oldFiles.ForEach(file => 
                {
                    try
                    {
                        MoveTo(file, zipDir);
                    }
                    catch (Exception ex2)
                    {
                        med.Err(ex2);
                    }
                });

                // Compress.
                string targetDir = Path.Combine(this.MessageFolder, "Backup", zipDir);
                string targetFile = zipDir + ".zip";
                string outputFile = Path.Combine(this.MessageFolder, "Backup", targetFile);
                try
                {
                    NLib.Utils.SevenZipManager.CompressDirectory(targetDir, outputFile, true);
                }
                catch (Exception ex3)
                {
                    med.Err(ex3);
                }

                // Remove Folder.
                try
                {
                    if (Directory.Exists(targetDir))
                    {
                        // Delete all sub directories and files
                        Directory.Delete(targetDir, true);
                    }
                }
                catch (Exception ex4)
                {
                    med.Err(ex4);
                }
            }
        }

        #endregion

        #region File Managements

        /// <summary>
        /// Write File to target folder.
        /// </summary>
        /// <param name="fileName">The file name with no extension.</param>
        /// <param name="message">The json data in string.</param>
        public void WriteFile(string fileName, string message)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(message))
                return;

            MethodBase med = MethodBase.GetCurrentMethod();
            string fullFileName = Path.Combine(this.FolderName, fileName + ".json");

            med.Info("Write message: {0}", message);
            med.Info("Attemp Generate file: {0}.", fileName + ".json");
            int iRetry = 0;
            // Save message.
            while (!File.Exists(fullFileName) && iRetry < 5)
            { 
                try
                {
                    using (var stream = File.CreateText(fullFileName))
                    {
                        stream.Write(message);
                        stream.Flush();
                        stream.Close();
                    }

                    var info = new FileInfo(fullFileName);
                    if (null != info)
                    {
                        med.Info("Generate file: {0}. File Size: {1:n0} bytes.", fileName + ".json", info.Length);
                    }
                }
                catch (Exception ex)
                {
                    med.Err(ex);
                    // remove if length is zero.
                    if (File.Exists(fullFileName))
                    {
                        var info = new FileInfo(fullFileName);
                        if (null == info || info.Length <= 0)
                        {
                            med.Info("Error whie Generate file: {0}.", fileName + ".json");
                            try
                            {
                                med.Info("Attemp to remove Generate file: {0}.", fileName + ".json");
                                File.Delete(fullFileName);
                            }
                            catch (Exception ex2)
                            {
                                med.Err(ex2);
                                med.Info("Failed to remove Generate file: {0}.", fileName + ".json");
                            }
                        }
                    }
                }
                ApplicationManager.Instance.Wait(50);
                iRetry++;
            }

            if (!File.Exists(fullFileName))
            {
                med.Info("Failed to Generate file: {0}.", fileName + ".json");
            }
            else
            {
                med.Info("Success to Generate file: {0}.", fileName + ".json");
            }
        }
        /// <summary>
        /// Move File to specificed sub folder.
        /// </summary>
        /// <param name="file">The target fule (Full File Name).</param>
        /// <param name="subFolder">The sub folder name.</param>
        protected void MoveTo(string file, string subFolder)
        {
            string parentDir = Path.GetDirectoryName(file);
            string fileName = Path.GetFileName(file);
            string targetPath = Path.Combine(parentDir, subFolder);
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
            if (!Directory.Exists(targetPath)) return;
            string targetFile = Path.Combine(targetPath, fileName);
            MethodBase med = MethodBase.GetCurrentMethod();
            try
            {
                if (File.Exists(targetFile)) File.Delete(targetFile);
                File.Move(file, targetFile);
            }
            catch (Exception ex)
            {
                med.Err(ex);
            }
        }
        /// <summary>
        /// Move File to 'Backup' sub folder.
        /// </summary>
        /// <param name="file">The target fule (Full File Name).</param>
        protected void MoveToBackup(string file)
        {
            MoveTo(file, "Backup");
        }
        /// <summary>
        /// Move File to 'Invalid' sub folder.
        /// </summary>
        /// <param name="file">The target fule (Full File Name).</param>
        protected void MoveToInvalid(string file)
        {
            MoveTo(file, "Invalid");
        }
        /// <summary>
        /// Move File to 'Error' sub folder.
        /// </summary>
        /// <param name="file">The target fule (Full File Name).</param>
        protected void MoveToError(string file)
        {
            MoveTo(file, "Error");
        }
        /// <summary>
        /// Move File to 'NotSupports' sub folder.
        /// </summary>
        /// <param name="file">The target fule (Full File Name).</param>
        protected void MoveToNotSupports(string file)
        {
            MoveTo(file, "NotSupports");
        }

        #endregion

        #region OnStart/OnShutdown

        /// <summary>
        /// OnStart.
        /// </summary>
        protected virtual void OnStart() { }
        /// <summary>
        /// OnShutdown.
        /// </summary>
        protected virtual void OnShutdown() { }

        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Start service.
        /// </summary>
        public void Start()
        {
            MethodBase med = MethodBase.GetCurrentMethod();
            // Init Scanning Timer
            _scanning = false;
            _timer = new Timer();
            _timer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();

            OnStart(); // call virtual method.
        }
        /// <summary>
        /// Shutdown service.
        /// </summary>
        public void Shutdown()
        {
            // Free Scanning Timer 
            try
            {
                if (null != _timer)
                {
                    _timer.Elapsed -= _timer_Elapsed;
                    _timer.Stop();
                    _timer.Dispose();
                }
            }
            catch { }
            _timer = null;
            _scanning = false;

            OnShutdown(); // call virtual method.
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets root message folder path name.
        /// </summary>
        public string MessageFolder
        {
            get
            {
                string localFilder = Folders.Combine(
                    Folders.Assemblies.CurrentExecutingAssembly, this.FolderName);
                if (!Folders.Exists(localFilder))
                {
                    Folders.Create(localFilder);
                }
                return localFilder;
            }
        }

        #endregion
    }
}
