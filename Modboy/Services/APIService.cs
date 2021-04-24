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
        private const string EndpointGetModInfo =
            "https://api.gamebanana.com/Core/Item/Data?" +
                "itemtype={0}" + "&" +
                "itemid={1}" + "&" +
                "fields=" +
                    "name," +
                    "Category().name," +
                    "RootCategory().name," +
                    "Url().sDownloadUrl()," +
                    "Game().name," + 
                    "Url().sProfileUrl()," +
                    "Url().sEmbeddablesUrl()," +
                    "Owner().name," +
                    "date" + "&" +
                "return_object=1" + "&" +
                "flags=JSON_UNESCAPED_SLASHES";

        private const string EndpointGetInstallationInstructions =
            "https://api.gamebanana.com/Core/Item/Data?" +
                "itemtype=File" + "&" +
                "itemid={0}" + "&" +
                "fields=" +
                    "sModManagerDownloadUrl()," +
                    "Modboy().aInstallationScheme()" + "&" +
                "return_object=1" + "&" +
                "flags=JSON_UNESCAPED_SLASHES";

        // ReSharper disable InconsistentNaming
        private const string URLAPIGetLastAppVersion = "http://api.gamebanana.com/Modboy/Version";
        private const string URLAPIReportException = "http://api.gamebanana.com/Modboy/Exception";
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
        public ModInfo GetModInfo((SubmissionType subType, string subId, string fileId) tuple)
        {
            string response = _webService.Get(string.Format(EndpointGetModInfo, tuple.subType, tuple.subId));
            if (response == null) return null;

            var modInfo = JsonConvert.DeserializeObject<ModInfo>(response);
            modInfo.SubmissionType = tuple.subType;
            modInfo.SubmissionId = tuple.subId;
            modInfo.FileId = tuple.fileId;

            return modInfo;
        }

        /// <summary>
        /// API Endpoint to get file info, including installation commands for a mod
        /// </summary>
        public FileInfo GetFileInfo(string fileId)
        {
            string response = _webService.Get(string.Format(EndpointGetInstallationInstructions, fileId));
            if (response == null) return null;

            return JsonConvert.DeserializeObject<FileInfo>(response);
        }

        /// <summary>
        /// Endpoint to report an exception to the server
        /// </summary>
        public void ReportException(string message, string systemInfo, Exception exception, string logDump, string databaseDump, string mailBack)
        {
            // Serialize
            string serializedData = JsonConvert.SerializeObject(new BugReportInfo
            {
                Message = message,
                SystemInfo = systemInfo,
                Exception = exception,
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
        public string GetLatestAvailableVersionDownloadUrl()
        {
            string response = _webService.Get(URLAPIGetLastAppVersion);
            if (response == null) return null;

            var versionData = JsonConvert.DeserializeObject<UpdateVersionInfo>(response);
            return versionData.DownloadUrl;
        }
    }
}