// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <Localization.cs>
//  Created By: Alexey Golub
//  Date: 25/02/2016
// ------------------------------------------------------------------ 

using System.Collections.Generic;
using System.IO;
using GalaSoft.MvvmLight;
using Modboy.Models.Database;
using Modboy.Models.Internal;
using NegativeLayer.Extensions;
using Newtonsoft.Json;

namespace Modboy
{
    public class Localization : ObservableObject
    {
        public const string DefaultLanguage = "English";
        private static bool LoadedDefaultOnce = false;

        /// <summary>
        /// Localization service that is currently in use
        /// </summary>
        public static Localization Current { get; } = new Localization();

        /// <summary>
        /// Get the list of available languages
        /// </summary>
        public static IEnumerable<string> GetAvailableLanguages()
        {
            // Custom files
            if (!Directory.Exists(FileSystem.LanguagesStorageDirectory))
            {
                yield break;
            }

            foreach (string languageFile in Directory.EnumerateFiles(FileSystem.LanguagesStorageDirectory, "*.lng"))
            {
                string name = Path.GetFileNameWithoutExtension(languageFile);
                yield return name;
            }
        }

        /// <summary>
        /// Gets the file, defining given language or null if not found
        /// </summary>
        public static string ResolveLanguageFilePath(string language)
        {
            string path = Path.Combine(FileSystem.LanguagesStorageDirectory, language + ".lng");
            return !File.Exists(path) ? null : path;
        }

        #region Language Strings
        // ReSharper disable InconsistentNaming
        public string LocalizationStringNotFound { get; set; } = "[STR_NOT_FOUND]";

        public string Global_OK { get; set; }
        public string Global_Cancel { get; set; }
        public string Global_Yes { get; set; }
        public string Global_No { get; set; }

        public string Command_Copy_SelectSource { get; set; }

        public string Task_Verify { get; set; }
        public string Task_Verify_Execute { get; set; }
        public string Task_Install { get; set; }
        public string Task_Install_Download { get; set; }
        public string Task_Install_Unpack { get; set; }
        public string Task_Install_Unpack_Failed { get; set; }
        public string Task_Install_Unpack_Failed_RAR5 { get; set; }
        public string Task_Install_Execute { get; set; }
        public string Task_Uninstall { get; set; }
        public string Task_GetModInfo { get; set; }
        public string Task_GetAffectedFiles { get; set; }
        public string Task_Uninstall_DeleteFiles { get; set; }
        public string Task_Uninstall_RestoreBackups { get; set; }
        public string Task_StoreResults { get; set; }
        public string Task_PromptUninstall { get; set; }
        public string Task_PromptReinstall { get; set; }
        public string Task_CommandExecutionFailed { get; set; }
        public string Task_UnknownInstallationProcess { get; set; }

        public string About_Title { get; set; }
        public string About_MoreInfo { get; set; }

        public string ComboSelect_Title { get; set; }

        public string History_TaskCompletedSuccessfully { get; set; }
        public string History_TaskCompletedUnsuccessfully { get; set; }
        public string History_Date { get; set; }
        public string History_Event { get; set; }
        public string History_Clear { get; set; }

        public string Main_Overview { get; set; }
        public string Main_History { get; set; }
        public string Main_Settings { get; set; }
        public string Main_TrayShowHide { get; set; }
        public string Main_TrayAbout { get; set; }
        public string Main_TrayHelp { get; set; }
        public string Main_TraySubmitBugReport { get; set; }
        public string Main_TrayExit { get; set; }

        public string Overview_NameFilter { get; set; }
        public string Overview_GameFilter { get; set; }
        public string Overview_Sorting { get; set; }
        public string Overview_InstallDate { get; set; }
        public string Overview_InQueue { get; set; }
        public string Overview_Installed { get; set; }
        public string Overview_OpenModPage { get; set; }
        public string Overview_Abort { get; set; }
        public string Overview_Verify { get; set; }
        public string Overview_Reinstall { get; set; }
        public string Overview_Uninstall { get; set; }
        public string Overview_VerifySuccessfulNotification { get; set; }
        public string Overview_VerifyUnsuccessfulNotification { get; set; }

        public string Overview_Type { get; set; }
        public string Overview_SubmissionId { get; set; }
        public string Overview_FileId { get; set; }

        public string ModSortingMethod_Name { get; set; }
        public string ModSortingMethod_AuthorName { get; set; }
        public string ModSortingMethod_GameName { get; set; }
        public string ModSortingMethod_InstallDate { get; set; }

        public string Prompt_Title { get; set; }
        public string Prompt_InvalidFile { get; set; }
        public string Prompt_InvalidDir { get; set; }

        public string Settings_General { get; set; }
        public string Settings_Aliases { get; set; }
        public string Settings_Language { get; set; }
        public string Settings_ComputerName { get; set; }
        public string Settings_TempDownloadPath { get; set; }
        public string Settings_UseBackup { get; set; }
        public string Settings_BackupPath { get; set; }
        public string Settings_ShowNotifications { get; set; }
        public string Settings_AutoUpdate { get; set; }
        public string Settings_SendBugReports { get; set; }
        public string Settings_AliasKeyword { get; set; }
        public string Settings_AliasValue { get; set; }
        public string Settings_Save { get; set; }
        public string Settings_Reset { get; set; }
        public string Settings_Cancel { get; set; }

        public string Updater_Title { get; set; }
        public string Updater_Status { get; set; }
        public string Updater_CheckFailed { get; set; }
        public string Updater_UpdateDownloadFailed { get; set; }
        public string Updater_UpdateInstallFailed { get; set; }
        public string Updater_UpdatePrompt { get; set; }

        public string BugReport_Title { get; set; }
        public string BugReport_Description { get; set; }
        public string BugReport_Message { get; set; }
        public string BugReport_IncludeLogs { get; set; }
        public string BugReport_IncludeDB { get; set; }
        public string BugReport_MailBack { get; set; }
        public string BugReport_SendReport { get; set; }
        public string BugReport_DiscardReport { get; set; }
        public string BugReport_NoMessageSpecified { get; set; }
        public string BugReport_NoMailBackSpecified { get; set; }
        public string BugReport_ReportSent { get; set; }
        public string BugReport_CopiedToClipboard { get; set; }

        // ReSharper restore InconsistentNaming
        #endregion

        public Localization()
        {
            // Load new localization when settings change
            Settings.Stager.Current.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Settings.Language))
                    LoadLanguageFile(Settings.Stager.Current.Language);
            };
        }

        /// <summary>
        /// Loads the specified language file by language name
        /// </summary>
        public bool LoadLanguageFile(string language)
        {
            if (DefaultLanguage != language & !LoadedDefaultOnce)
            {
                LoadLanguageFile(DefaultLanguage);
                LoadedDefaultOnce = true;
            }

            // Find file
            string filePath = ResolveLanguageFilePath(language);
            if (filePath.IsBlank()) return false;
            if (!File.Exists(filePath)) return false;

            // Load data
            string data = File.ReadAllText(filePath);
            JsonConvert.PopulateObject(data, this);

            // Raise a change event for all properties
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty);
            return true;
        }

        public string Localize(TaskExecutionStatus value)
        {
            switch (value)
            {
                case TaskExecutionStatus.None:
                    return string.Empty;
                case TaskExecutionStatus.Verify:
                    return Task_Verify;
                case TaskExecutionStatus.VerifyGetModInfo:
                    return Task_GetModInfo;
                case TaskExecutionStatus.VerifyGetAffectedFiles:
                    return Task_GetAffectedFiles;
                case TaskExecutionStatus.VerifyExecute:
                    return Task_Verify_Execute;
                case TaskExecutionStatus.Install:
                    return Task_Install;
                case TaskExecutionStatus.InstallGetModInfo:
                    return Task_GetModInfo;
                case TaskExecutionStatus.InstallDownload:
                    return Task_Install_Download;
                case TaskExecutionStatus.InstallUnpack:
                    return Task_Install_Unpack;
                case TaskExecutionStatus.InstallExecute:
                    return Task_Install_Execute;
                case TaskExecutionStatus.InstallStoreResults:
                    return Task_StoreResults;
                case TaskExecutionStatus.Uninstall:
                    return Task_Uninstall;
                case TaskExecutionStatus.UninstallGetModInfo:
                    return Task_GetModInfo;
                case TaskExecutionStatus.UninstallGetAffectedFiles:
                    return Task_GetAffectedFiles;
                case TaskExecutionStatus.UninstallDeleteFiles:
                    return Task_Uninstall_DeleteFiles;
                case TaskExecutionStatus.UninstallRestoreBackups:
                    return Task_Uninstall_RestoreBackups;
                case TaskExecutionStatus.UninstallStoreResults:
                    return Task_StoreResults;
                default:
                    return LocalizationStringNotFound;
            }
        }

        public string Localize(OverviewSortingMethod value)
        {
            switch (value)
            {
                case OverviewSortingMethod.Name:
                    return ModSortingMethod_Name;
                case OverviewSortingMethod.AuthorName:
                    return ModSortingMethod_AuthorName;
                case OverviewSortingMethod.GameName:
                    return ModSortingMethod_GameName;
                case OverviewSortingMethod.InstallDate:
                    return ModSortingMethod_InstallDate;
                default:
                    return LocalizationStringNotFound;
            }
        }

        public string Localize(HistoryEntry value)
        {
            // Get string
            string str = value.Success
                ? History_TaskCompletedSuccessfully
                : History_TaskCompletedUnsuccessfully;

            // Format it
            return str.Format(value.TaskType, value.ModName);
        }
    }
}