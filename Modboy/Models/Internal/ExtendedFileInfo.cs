// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <ExtendedFileInfo.cs>
//  Created By: Steve Elliott
//  Date: 28/11/2017
// ------------------------------------------------------------------ 

using System.IO;
using System.Net;

namespace Modboy.Models.Internal
{
    public class ExtendedFileInfo
    {
        public FileInfo FileInfo { get; private set; }
        public WebHeaderCollection ResponseHeaders { get; private set; }

        public ExtendedFileInfo(FileInfo fileInfo, WebHeaderCollection responseHeaders)
        {
            FileInfo = fileInfo;
            ResponseHeaders = responseHeaders;
        }
    }
}
