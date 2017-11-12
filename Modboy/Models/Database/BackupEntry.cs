﻿// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <BackupEntry.cs>
//  Created By: Alexey Golub
//  Date: 21/02/2016
// ------------------------------------------------------------------ 

using System.IO;
using NegativeLayer.Extensions;
using SQLite;

namespace Modboy.Models.Database
{
    public class BackupEntry
    {
        [PrimaryKey, AutoIncrement]
        public int RowID { get; set; }

        public string OriginalFilePath { get; set; }
        public string OriginalFileName { get; set; }
        public string BackupFileName { get; set; }
        public long InstallOrder { get; set; }
        public string ModId { get; set; }

        public BackupEntry(string originalFilePath, long installOrder, string modId)
        {
            OriginalFilePath = originalFilePath;
            OriginalFileName = Path.GetFileName(OriginalFilePath);
            BackupFileName = $"{originalFilePath.ToSHA1Hash()}_{modId}_.bak";
            InstallOrder = installOrder;
            ModId = modId;
        }

        public BackupEntry(string originalFilePath, string backupFileName, long installOrder, string modId)
        {
            OriginalFilePath = originalFilePath;
            OriginalFileName = Path.GetFileName(originalFilePath);
            BackupFileName = backupFileName;
            InstallOrder = installOrder;
            ModId = modId;
        }

        public BackupEntry() { }
    }
}