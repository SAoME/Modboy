// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <PersistenceService.cs>
//  Created By: Alexey Golub
//  Date: 21/02/2016
// ------------------------------------------------------------------ 

using System;
using System.Linq;
using Modboy.Models.Database;
using Modboy.Models.Internal;

namespace Modboy.Services
{
    public class PersistenceService
    {
        private readonly DatabaseService _databaseService;

        public PersistenceService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Get the list of all currently installed mods
        /// </summary>
        public InstalledModEntry[] GetInstalledMods()
        {
            return _databaseService.DB.Table<InstalledModEntry>().ToArray();
        }

        /// <summary>
        /// Find a record for the given mod
        /// </summary>
        public InstalledModEntry GetInstalledMod(string fileId)
        {
            // TODO: Sufficient?
            return _databaseService.DB.Find<InstalledModEntry>(e => e.FileId == fileId);
        }

        /// <summary>
        /// Record the fact of successful installation to the database
        /// </summary>
        public void RecordInstall((SubmissionType subType, string subId, string fileId) tuple, string[] fileChanges, DateTime date)
        {
            _databaseService.DB.Insert(new InstalledModEntry(tuple, fileChanges, date));

            Logger.Record($"Added installed mod entry to the database ({tuple.subType} {tuple.subId}, file: {tuple.fileId})");
        }

        /// <summary>
        /// Record the fact of successful installation to the database
        /// </summary>
        public void RecordInstall((SubmissionType, string, string) tuple, string[] fileChanges)
        {
            RecordInstall(tuple, fileChanges, DateTime.Now);
        }

        /// <summary>
        /// Record the fact of successful uninstallation to the database
        /// </summary>
        public void RecordUninstall((SubmissionType subType, string subId, string fileId) tuple)
        {
            var entry = GetInstalledMod(tuple.fileId);
            if (entry != null)
                _databaseService.DB.Delete(entry);

            Logger.Record($"Removed installed mod entry from the database ({tuple.subType} {tuple.subId}, file: {tuple.fileId})");
        }

        /// <summary>
        /// Get the files affected by given mod's installation
        /// </summary>
        public string[] GetAffectedFiles(string fileId)
        {
            var entry = GetInstalledMod(fileId);
            return entry?.FileChanges;
        }

        /// <summary>
        /// Checks if the given mod is installed
        /// </summary>
        public bool IsInstalled(string fileId)
        {
            return GetInstalledMod(fileId) != null;
        }
    }
}