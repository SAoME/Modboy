// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <SettingsViewModel.cs>
//  Created By: Alexey Golub
//  Date: 24/02/2016
// ------------------------------------------------------------------ 

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using Modboy.Models.API;
using Modboy.Models.Internal;
using Modboy.Services;
using NegativeLayer.Extensions;
using NegativeLayer.Settings;
using NegativeLayer.WPFExtensions;

namespace Modboy.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly AliasService _aliasService;
        private readonly WindowService _windowService;

        public Localization Localization { get; } = Localization.Current;
        public SettingsManagerStager<Settings> Stager { get; } = Settings.Stager;
        public Settings CurrentSettings => Stager.Current;
        public Settings StagingSettings => Stager.Staging;

        public IEnumerable<string> AvailableLanguages => Localization.GetAvailableLanguages();
        public BindingList<GlobalAlias> UserAliases { get; } = new BindingList<GlobalAlias>();

        private bool _userAliasesHandled;

        // Commands
        public RelayCommand LocateTempDownloadPathCommand { get; }
        public RelayCommand LocateBackupPathCommand { get; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand ResetDefaultsCommand { get; }
        public RelayCommand CancelCommand { get; }

        public SettingsViewModel(AliasService aliasService, WindowService windowService)
        {
            _aliasService = aliasService;
            _windowService = windowService;

            // Commands
            LocateTempDownloadPathCommand = new RelayCommand(LocateTempDownloadPath);
            LocateBackupPathCommand = new RelayCommand(LocateBackupPath, () => StagingSettings.UseBackup);

            SaveCommand = new RelayCommand(Save, () => !StagingSettings.IsSaved);
            ResetDefaultsCommand = new RelayCommand(ResetDefaults);
            CancelCommand = new RelayCommand(Cancel, () => !StagingSettings.IsSaved);

            // Events
            _aliasService.UserAliasesChanged += (sender, args) => GetUserAliases();
            UserAliases.ListChanged += (sender, args) => PutUserAliases();

            // Initial
            GetUserAliases();
        }

        private void LocateTempDownloadPath()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            StagingSettings.TempDownloadPath = dialog.SelectedPath;
            RaisePropertyChanged(nameof(StagingSettings));
        }

        private void LocateBackupPath()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            StagingSettings.BackupPath = dialog.SelectedPath;
        }

        private void Save()
        {
            Stager.Save();
        }

        private void ResetDefaults()
        {
            StagingSettings.Reset();
        }

        private void Cancel()
        {
            Stager.RevertStaged();
        }

        private void GetUserAliases()
        {
            if (_userAliasesHandled) return;
            _userAliasesHandled = true;

            var userAliasList = _aliasService.UserAliases.ToArray().Select(kvp => new GlobalAlias(kvp.Key, kvp.Value)).ToList();
            DispatcherHelper.UIDispatcher.InvokeSafe(() =>
            {
                UserAliases.Clear();
                userAliasList.ForEach(UserAliases.Add);
            });

            _userAliasesHandled = false;
        }

        private void PutUserAliases()
        {
            if (_userAliasesHandled) return;
            _userAliasesHandled = true;

            _aliasService.Populate(UserAliases);

            _userAliasesHandled = false;
        }
    }
}