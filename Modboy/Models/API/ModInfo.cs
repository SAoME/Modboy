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
using Modboy.Models.Internal;
using Modboy.Extensions;

namespace Modboy.Models.API
{
    public class ModInfo : ObservableObject
    {
        private SubmissionType _submissionType;
        private string _submissionId;
        private string _fileId;

        private string _categoryName;
        private string _gameName;
        private string _ownerName;
        private string _name;
        private DateTime _timeAdded;

        private string _imageUrl;
        private string _downloadUrl;
        private string _pageUrl;

        public (SubmissionType submissionType, string submissionId, string fileId) Identifier => (submissionType: SubmissionType, submissionId: SubmissionId, fileId: FileId);

        [JsonProperty("submissionType")]
        public SubmissionType SubmissionType
        {
            get { return _submissionType; }
            set { Set(ref _submissionType, value); }
        }

        [JsonProperty("submissionId")]
        public string SubmissionId
        {
            get { return _submissionId; }
            set { Set(ref _submissionId, value); }
        }

        [JsonProperty("fileId")]
        public string FileId
        {
            get { return _fileId; }
            set { Set(ref _fileId, value); }
        }

        [JsonProperty("Category().name")]
        public string CategoryName
        {
            get { return _categoryName; }
            set { Set(ref _categoryName, value); }
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

        [JsonProperty("description")]
        public string Description => $"{GameName} {CategoryName} {SubmissionType}";

        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        [JsonProperty("date")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime TimeAdded
        {
            get { return _timeAdded; }
            set { Set(ref _timeAdded, value); }
        }

        [JsonProperty("Url().sEmbeddablesUrl()")]
        public string ImageUrl
        {
            get { return _imageUrl?.TryAppend("?type=medium_square_minimal"); }
            set { Set(ref _imageUrl, value); }
        }

        [JsonProperty("Url().sGetDownloadUrl()")]
        public string DownloadUrl
        {
            get { return _downloadUrl; }
            set { Set(ref _downloadUrl, value); }
        }

        [JsonProperty("Url().sGetProfileUrl()")]
        public string PageUrl
        {
            get { return _pageUrl; }
            set { Set(ref _pageUrl, value); }
        }

        public ModInfo() { }

        public ModInfo((SubmissionType subType, string subId, string fileId) tuple)
        {
            SubmissionType = tuple.subType;
            SubmissionId = tuple.subId;
            FileId = tuple.fileId;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}