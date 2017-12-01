// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <BugReportInfo.cs>
//  Created By: Steve Elliott
//  Date: 30/11/2017
// ------------------------------------------------------------------ 

using System;

namespace Modboy.Models.API
{
    public class BugReportInfo
    {
        public string Version { get; } = App.CurrentVersion.ToString();
        public DateTime ReportDateUTC { get; } = DateTime.UtcNow;
        public string Message { get; set; }
        public string SystemInfo { get; set; }
        private Exception _exception;
        public Exception Exception
        {
            get => _exception;
            set => _exception = value ?? new Exception("User-initiated bug report");
        }
        public string Log { get; set; }
        public string Database { get; set; }
        public string MailBack { get; set; }
    }
}
