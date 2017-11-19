// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <FileInfo.cs>
//  Created By: Steve Elliott
//  Date: 18/11/2017
// ------------------------------------------------------------------ 

using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Modboy.Models.API
{
    public class FileInfo : ObservableObject
    {
        private string _downloadUrl;
        private List<Command> _installationCommands;

        [JsonProperty("sModManagerDownloadUrl()")]
        public string DownloadUrl
        {
            get { return _downloadUrl; }
            set { Set(ref _downloadUrl, value); }
        }

        [JsonProperty("Modboy().aInstallationScheme()")]
        public List<Command> InstallationCommands
        {
            get { return _installationCommands; }
            set { Set(ref _installationCommands, value); }
        }
    }
}
