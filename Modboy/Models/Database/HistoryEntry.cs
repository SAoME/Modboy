﻿// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <HistoryEntry.cs>
//  Created By: Alexey Golub
//  Date: 06/03/2016
// ------------------------------------------------------------------ 

using System;
using Modboy.Models.Internal;
using SQLite;

namespace Modboy.Models.Database
{
    public class HistoryEntry
    {
        [PrimaryKey, AutoIncrement]
        public int RowID { get; set; }

        public DateTime Date { get; set; }
        public TaskType TaskType { get; set; }
        public bool Success { get; set; }
        public string ModId { get; set; }
        public string ModName { get; set; }
		public string ModUrl { get; set; }

        public HistoryEntry(DateTime date, TaskType taskType, bool success, string modId, string modName, string modUrl)
        {
            Date = date;
            TaskType = taskType;
            Success = success;
            ModId = modId;
            ModName = modName ?? modId;
			ModUrl = modUrl;
        }

        public HistoryEntry() { }
    }
}