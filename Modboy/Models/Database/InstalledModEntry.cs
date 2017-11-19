// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <InstalledModEntry.cs>
//  Created By: Alexey Golub
//  Date: 21/02/2016
// ------------------------------------------------------------------ 

using System;
using System.Collections.Generic;
using NegativeLayer.Extensions;
using SQLite;
using Modboy.Models.Internal;

namespace Modboy.Models.Database
{
    public class InstalledModEntry
    {
        [PrimaryKey, AutoIncrement]
        public int RowID { get; set; }

        public SubmissionType SubmissionType { get; set; }
        public string SubmissionId { get; set; }
        public string FileId { get; set; }

        public (SubmissionType submissionType, string submissionId, string fileId) Identifier => (submissionType: SubmissionType, submissionId: SubmissionId, fileId: FileId);

        public string AffectedFilesStr { get; set; }
        public string[] FileChanges => AffectedFilesStr.SplitTrim(Constants.UniformSeparator);
        public DateTime InstallDate { get; set; }

        public InstalledModEntry((SubmissionType subType, string subId, string fileId) tuple, string fileChangesStr, DateTime installDate)
        {
            SubmissionType = tuple.subType;
            SubmissionId = tuple.subId;
            FileId = tuple.fileId;
            AffectedFilesStr = fileChangesStr;
            InstallDate = installDate;
        }

        public InstalledModEntry((SubmissionType subType, string subId, string fileId) tuple, IEnumerable<string> fileChanges, DateTime installDate)
        {
            SubmissionType = tuple.subType;
            SubmissionId = tuple.subId;
            FileId = tuple.fileId;
            AffectedFilesStr = fileChanges.JoinToString(Constants.UniformSeparator);
            InstallDate = installDate;
        }

        public InstalledModEntry((SubmissionType subType, string subId, string fileId) tuple)
        {
            SubmissionType = tuple.subType;
            SubmissionId = tuple.subId;
            FileId = tuple.fileId;
        }

        public InstalledModEntry() { }
    }
}