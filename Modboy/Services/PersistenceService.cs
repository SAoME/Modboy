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
        public InstalledModEntry GetInstalledMod(string modId)
        {
            return _databaseService.DB.Find<InstalledModEntry>(e => e.ModId == modId);
        }

        /// <summary>
        /// Record the fact of successful installation to the database
        /// </summary>
        public void RecordInstall(string modId, string[] fileChanges, DateTime date)
        {
            _databaseService.DB.Insert(new InstalledModEntry(modId, fileChanges, date));

            Logger.Record($"Added installed mod entry to the database (#{modId})");
        }

        /// <summary>
        /// Record the fact of successful installation to the database
        /// </summary>
        public void RecordInstall(string modId, string[] fileChanges)
        {
            RecordInstall(modId, fileChanges, DateTime.Now);
        }

        /// <summary>
        /// Record the fact of successful uninstallation to the database
        /// </summary>
        public void RecordUninstall(string modId)
        {
            var entry = GetInstalledMod(modId);
            if (entry != null)
                _databaseService.DB.Delete(entry);

            Logger.Record($"Removed installed mod entry from the database (#{modId})");
        }

        /// <summary>
        /// Get the files affected by given mod's installation
        /// </summary>
        public string[] GetAffectedFiles(string modId)
        {
            var entry = GetInstalledMod(modId);
            return entry?.FileChanges;
        }

        /// <summary>
        /// Checks if the given mod is installed
        /// </summary>
        public bool IsInstalled(string modId)
        {
            return GetInstalledMod(modId) != null;
        }
    }
}