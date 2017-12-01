// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <Constants.cs>
//  Created By: Alexey Golub
//  Date: 13/02/2016
// ------------------------------------------------------------------ 

using System.Diagnostics.CodeAnalysis;

namespace Modboy
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Constants
    {
        public const int ExceptionReportLogDumpLines = 100;
        public const string ProtocolHandle = "modboy://";
        public const string UniformSeparator = "|";
        public static readonly string UserAgent = "Modboy (ver " + App.CurrentVersion + ")";

        public static readonly string URLAboutPage = "https://gamebanana.com/tools/6321";
        public static readonly string URLHelp = "https://gamebanana.com/wikis/1952";
        public static readonly string URLSubmitBugReport = "https://gamebanana.com/pms/add?name=Modboy+Bug&recipients=142413&article=Paste+report+below:";
    }
}