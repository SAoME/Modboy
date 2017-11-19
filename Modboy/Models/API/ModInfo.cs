// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <ModInfo.cs>
//  Created By: Alexey Golub
//  Date: 13/02/2016
// ------------------------------------------------------------------ 

using System;
using GalaSoft.MvvmLight;
using Modboy.Models.Converters;
using Newtonsoft.Json;

namespace Modboy.Models.API
{
    public class ModInfo : ObservableObject
    {
        private string _modId;
        private string _gameName;
        private string _ownerName;
        private string _description;
        private string _name;
        private DateTime _timeAdded;
        private string _imageUrl;
        private string _downloadUrl;
        private string _pageUrl;

        [JsonProperty("ID")]
        public string ModId
        {
            get { return _modId; }
            set { Set(ref _modId, value); }
        }

        [JsonProperty("Game().name")]
        public string GameName
        {
            get { return _gameName; }
            set { Set(ref _gameName, value); }
        }

        [JsonProperty("Owner().name")]
        public string OwnerName
        {
            get { return _ownerName; }
            set { Set(ref _ownerName, value); }
        }

        [JsonProperty("Description")]
        public string Description
        {
            get { return _description; }
            set { Set(ref _description, value); }
        }

        [JsonProperty("Name")]
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        [JsonProperty("TimeAdded")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime TimeAdded
        {
            get { return _timeAdded; }
            set { Set(ref _timeAdded, value); }
        }

        [JsonProperty("ThumbnailUrl")]
        public string ImageUrl
        {
            get { return _imageUrl; }
            set { Set(ref _imageUrl, value); }
        }

        [JsonProperty("DownloadUrl")]
        public string DownloadUrl
        {
            get { return _downloadUrl; }
            set { Set(ref _downloadUrl, value); }
        }

        [JsonProperty("ProfileUrl")]
        public string PageUrl
        {
            get { return _pageUrl; }
            set { Set(ref _pageUrl, value); }
        }

        public ModInfo() { }

        public ModInfo(string modId)
        {
            ModId = modId;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}