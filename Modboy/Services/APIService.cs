// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <APIService.cs>
//  Created By: Alexey Golub
//  Date: 14/02/2016
// ------------------------------------------------------------------ 

using NegativeLayer.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Modboy.Models.API;
using Modboy.Models.Internal;

namespace Modboy.Services
{
    public class APIService
    {
        // ReSharper disable InconsistentNaming
        private const string URLAPIGetInstallCommands =
            "http://api.gamebanana.com/Modboy/InstallMod?id={0}&os=windows";
        private const string URLAPIGetVerifyData = "http://api.gamebanana.com/Modboy/VerifyMod?id={0}&os=windows";
        private const string URLAPIResolveModID = "http://api.gamebanana.com/Modboy/ModInfo?id={0}";
        private const string URLAPIGetLastAppVersion = "http://api.gamebanana.com/Modboy/Version";
        private const string URLAPIReportException = "http://api.gamebanana.com/Modboy/Exception";
        private const string URLAPIReportSuccessfulTask = "http://api.gamebanana.com/Modboy/Sync";
        // ReSharper restore InconsistentNaming

        private readonly WebService _webService = new WebService();

        /// <summary>
        /// Function, that will be periodically executed to determine if an abort is issued
        /// </summary>
        public Func<bool> AbortChecker { get; set; } = () => false;

        public APIService()
        {
            // Delegates
            _webService.AbortChecker = AbortChecker;
        }

        /// <summary>
        /// API Endpoint to get the mod info by its ID
        /// </summary>
        public ModInfo GetModInfo(string modID)
        {
            string response = _webService.Get(string.Format(URLAPIResolveModID, modID));
            if (response == null) return null;

            var modInfo = JsonConvert.DeserializeObject<ModInfo>(response);
            modInfo.ModID = modID;

            return modInfo;
        }

        /// <summary>
        /// API Endpoint to get verification hashes for a mod
        /// </summary>
        public IReadOnlyList<VerificationPair> GetVerificationPairs(string modID)
        {
            string response = _webService.Get(string.Format(URLAPIGetVerifyData, modID));
            if (response == null) return null;

            return JsonConvert.DeserializeObject<VerificationPair[]>(response);
        }

        /// <summary>
        /// API Endpoint to get installation commands for a mod
        /// </summary>
        public IReadOnlyList<Command> GetInstallationCommands(string modID)
        {
            string response = _webService.Get(string.Format(URLAPIGetInstallCommands, modID));
            if (response == null) return null;

            return JsonConvert.DeserializeObject<Command[]>(response);
        }

        /// <summary>
        /// API Endpoint to report successful execution of a task
        /// </summary>
        public void ReportSuccessfulExecution(string modID, TaskType taskType)
        {
            // Serialize status data
            string serializedData = JsonConvert.SerializeObject(new
            {
                Settings.Stager.Current.ComputerName,
                UserID = 0.ToString(),
                Action = taskType.ToString(),
                ModID = modID
            });

            // To base64
            string base64Data = serializedData.ToBytes().ToBase64();

            // Send them to server
            _webService.Post(URLAPIReportSuccessfulTask, new Dictionary<string, string> {{"data", base64Data}});
        }

        /// <summary>
        /// Endpoint to report an exception to the server
        /// </summary>
        public void ReportException(string message, string systemInfo, Exception exception, string logDump, string databaseDump, string mailBack)
        {
            // Serialize
            string serializedData = JsonConvert.SerializeObject(new
            {
                Version = App.CurrentVersion.ToString(),
                ReportDateUTC = DateTime.UtcNow,
                User = 0.ToString(),
                Message = message,
                SystemInfo = systemInfo,
                Exception = exception ?? new Exception("User-initiated bug report"),
                Log = logDump,
                Database = databaseDump,
                MailBack = mailBack
            });

            // Send data to server
            string base64Data = serializedData.ToBytes().ToBase64();
            _webService.Post(URLAPIReportException, new Dictionary<string, string> {{"data", base64Data}});
        }

        /// <summary>
        /// API Endpoint to get the latest available software version
        /// </summary>
        public Version GetLatestAvailableVersion()
        {
            string response = _webService.Get(URLAPIGetLastAppVersion);
            if (response == null) return null;

            var versionData = JsonConvert.DeserializeObject<UpdateVersionInfo>(response);
            return versionData.Version;
        }

        /// <summary>
        /// API Endpoint to get the latest available software version download link
        /// </summary>
        /// <returns></returns>
        public string GetLatestAvailableVersionDownloadURL()
        {
            string response = _webService.Get(URLAPIGetLastAppVersion);
            if (response == null) return null;

            var versionData = JsonConvert.DeserializeObject<UpdateVersionInfo>(response);
            return versionData.DownloadURL;
        }
    }
}