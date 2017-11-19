// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <StringExtensions.cs>
//  Created By: Steve Elliott
//  Date: 14/11/2017
// ------------------------------------------------------------------

namespace Modboy.Extensions
{
    public static class StringExtensions
    {
        public static string TryAppend(this string subject, string toAppend)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
               return null;
            }

            if (string.IsNullOrWhiteSpace(toAppend))
            {
                toAppend = string.Empty;
            }

            return subject + toAppend;
        }
    }
}
