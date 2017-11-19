// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <OverviewViewModel.cs>
//  Created By: Alexey Golub
//  Date: 13/02/2016
// ------------------------------------------------------------------ 

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using Modboy.Models.Internal;
using Modboy.Services;
using NegativeLayer.Extensions;
using NegativeLayer.WPFExtensions;
using Task = Modboy.Models.Internal.Task;
using OrigTask = System.Threading.Tasks.Task;

namespace Modboy.ViewModels
{
    public class OverviewViewModel : ViewModelBase
    {
        private readonly TaskExecutionService _taskExecutionService;
        private readonly PersistenceService _persistenceService;
        private readonly WindowService _windowService;
        private readonly APIService _apiService = new APIService();

        private readonly TaskFactory _taskFactory = new TaskFactory(TaskCreationOptions.PreferFairness,
            TaskContinuationOptions.PreferFairness);
        private readonly ICollectionView _collectionView;

        public Localization Localization { get; } = Localization.Current;

        public ObservableCollection<ModStatus> Mods { get; } = new ObservableCollection<ModStatus>();
        public ModStatus ExpandedModStatus { get; private set; }

        private string[] _availableGameFilters;
        public string[] AvailableGameFilters
        {
            get { return _availableGameFilters; }
            private set { Set(ref _availableGameFilters, value); }
        }

        private string[] _availableSortingMethods;
        public string[] AvailableSortingMethods
        {
            get { return _availableSortingMethods; }
            private set { Set(ref _availableSortingMethods, value); }
        }

        private string _nameFilter;
        public string NameFilter
        {
            get { return _nameFilter; }
            set
            {
                Set(ref _nameFilter, value);
                UpdateFilter();
            }
        }

        private string _gameFilter;
        public string GameFilter
        {
            get { return _gameFilter; }
            set
            {
                Set(ref _gameFilter, value);
                UpdateFilter();
            }
        }

        private OverviewSortingMethod _sortingMethod = OverviewSortingMethod.Name;
        public OverviewSortingMethod SortingMethod
        {
            get { return _sortingMethod; }
            set
            {
                Set(ref _sortingMethod, value);
                RaisePropertyChanged(() => SortingMethodIndex);
                UpdateSorting();
            }
        }

        public int SortingMethodIndex
        {
            get { return (int) SortingMethod; }
            set { SortingMethod = (OverviewSortingMethod) value; }
        }

        // Commands
        public RelayCommand<ModStatus> ToggleExpandCommand { get; }
        public RelayCommand<ModStatus> ExpandCommand { get; }
        public RelayCommand<ModStatus> UnexpandCommand { get; }
        public RelayCommand ClearFilters { get; }
        public RelayCommand<(SubmissionType, string, string)> AbortCommand { get; }
        public RelayCommand<(SubmissionType, string, string)> OpenModPageCommand { get; }
        public RelayCommand<(SubmissionType, string, string)> VerifyCommand { get; }
        public RelayCommand<(SubmissionType, string, string)> ReinstallCommand { get; }
        public RelayCommand<(SubmissionType, string, string)> UninstallCommand { get; }

        public OverviewViewModel(TaskExecutionService taskExecutionService, PersistenceService persistenceService, WindowService windowService)
        {
            _taskExecutionService = taskExecutionService;
            _persistenceService = persistenceService;
            _windowService = windowService;

            // Setup collection view
            _collectionView = CollectionViewSource.GetDefaultView(Mods);

            // Commands
            ToggleExpandCommand = new RelayCommand<ModStatus>(ToggleExpand);
            ExpandCommand = new RelayCommand<ModStatus>(Expand);
            UnexpandCommand = new RelayCommand<ModStatus>(Unexpand);
            ClearFilters = new RelayCommand(() => NameFilter = GameFilter = null,
                () => NameFilter.IsNotBlank() || GameFilter.IsNotBlank());
            AbortCommand = new RelayCommand<(SubmissionType, string, string)>(Abort);
            OpenModPageCommand = new RelayCommand<(SubmissionType, string, string)>(OpenModPage);
            VerifyCommand = new RelayCommand<(SubmissionType, string, string)>(Verify);
            ReinstallCommand = new RelayCommand<(SubmissionType, string, string)>(Reinstall);
            UninstallCommand = new RelayCommand<(SubmissionType, string, string)>(Uninstall);

            // Events
            Localization.PropertyChanged += (sender, args) =>
            {
                UpdateLocalization();
                PopulateAll();
            };
            _taskExecutionService.TaskAddedToQueue += (sender, args) => PopulateEnqueuedMods();
            _taskExecutionService.TaskStateChanged += (sender, args) => PopulateCurrent();
            _taskExecutionService.TaskEnded += (sender, args) =>
            {
                // If it was a verification task - notify of the result
                if (args.Task.TaskType == TaskType.Verify)
                {
                    if (args.Success)
                        _windowService.ShowNotificationWindowAsync(Localization.Overview_VerifySuccessfulNotification).GetResult();
                    else
                        _windowService.ShowErrorWindowAsync(Localization.Overview_VerifyUnsuccessfulNotification).GetResult();
                }

                RemoveModStatus(args.Task.Identifier);
                PopulateInstalledMod(args.Task.Identifier);
            };
            _taskExecutionService.TaskAborted += (sender, args) =>
            {
                RemoveModStatus(args.Task.Identifier);
                PopulateInstalledMod(args.Task.Identifier);
            };
            _taskExecutionService.TaskRemovedFromQueue += (sender, args) =>
            {
                RemoveModStatus(args.Task.Identifier);
                PopulateInstalledMod(args.Task.Identifier);
            };

            // Initial population
            UpdateLocalization();
            PopulateAll();
        }

        private void UpdateLocalization()
        {
            var lastSortingMethod = SortingMethod;
            AvailableSortingMethods =
                typeof(OverviewSortingMethod).GetAllEnumValues<OverviewSortingMethod>()
                    .Select(sm => Localization.Localize(sm))
                    .ToArray();
            SortingMethod = lastSortingMethod;
        }

        private void UpdateFilter()
        {
            if (NameFilter.IsBlank() && GameFilter.IsBlank())
            {
                _collectionView.Filter = null;
            }
            else
            {
                _collectionView.Filter = o =>
                {
                    var status = (ModStatus) o;
                    if (NameFilter.IsNotBlank() && !status.ModInfo.Name.ContainsInvariant(NameFilter))
                        return false;
                    if (GameFilter.IsNotBlank() && !status.ModInfo.GameName.ContainsInvariant(GameFilter))
                        return false;
                    return true;
                };
            }
            ClearFilters.RaiseCanExecuteChanged();
        }

        private void UpdateSorting()
        {
            string propertyName;
            switch (SortingMethod)
            {
                case OverviewSortingMethod.Name:
                    propertyName = "ModInfo.Name";
                    break;
                case OverviewSortingMethod.AuthorName:
                    propertyName = "ModInfo.Author.Name";
                    break;
                case OverviewSortingMethod.GameName:
                    propertyName = "ModInfo.Game.Name";
                    break;
                case OverviewSortingMethod.InstallDate:
                    propertyName = "InstalledModEntry.InstallDate";
                    break;
                default:
                    propertyName = null;
                    break;
            }

            _collectionView.SortDescriptions.Clear();
            if (propertyName.IsBlank())
                return;
            _collectionView.SortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Ascending));
        }

        private void RefreshCollectionView()
        {
            // Re-sort, re-filter
            _collectionView.Refresh();

            // Update game list
            var games = Mods.Select(m => m.ModInfo.GameName).Distinct();
            AvailableGameFilters = new[] { string.Empty }.Union(games).ToArray();
        }

        private void Expand(ModStatus modStatus)
        {
            if (ExpandedModStatus == modStatus)
                return;
            if (ExpandedModStatus != null)
                Unexpand(modStatus);
            ExpandedModStatus = modStatus;
            ExpandedModStatus.IsExpanded = true;
        }

        private void Unexpand(ModStatus modStatus)
        {
            if (ExpandedModStatus == modStatus)
                ExpandedModStatus = null;
            modStatus.IsExpanded = false;
        }

        private void ToggleExpand(ModStatus modStatus)
        {
            if (modStatus.IsExpanded)
                Unexpand(modStatus);
            else
                Expand(modStatus);
        }

        private void Abort((SubmissionType, string, string) tuple)
        {
            _taskExecutionService.AbortTask(tuple);

            // Update status
            DispatcherHelper.UIDispatcher.InvokeSafe(() =>
            {
                var modStatus = GetModStatus(tuple);
                if (modStatus != null)
                    modStatus.IsAborted = true;
            });
        }

        private void OpenModPage((SubmissionType subType, string subId, string fileId) tuple)
        {
            var modStatus = GetModStatus(tuple);
            if (modStatus == null || modStatus.ModInfo.PageUrl.IsBlank()) return;
            try
            {
                Process.Start(modStatus.ModInfo.PageUrl);
            }
            catch (Exception ex)
            {
                Logger.Record($"Could not open mod page for {tuple.subType} {tuple.subId}, file: {tuple.fileId}");
                Logger.Record(ex);
            }
        }

        private void Verify((SubmissionType, string, string) tuple)
        {
            _taskExecutionService.EnqueueTask(new Task(TaskType.Verify, tuple));
        }

        private void Reinstall((SubmissionType, string, string) tuple)
        {
            _taskExecutionService.EnqueueTask(new Task(TaskType.Install, tuple));
        }

        private void Uninstall((SubmissionType, string, string) tuple)
        {
            _taskExecutionService.EnqueueTask(new Task(TaskType.Uninstall, tuple));
        }

        private async OrigTask PopulateModInfoAsync((SubmissionType, string, string) tuple)
        {
            var populatedModInfo = await _taskFactory.StartNew(() => _apiService.GetModInfo(tuple));
            if (populatedModInfo == null) return;

            // Synchronize
            await DispatcherHelper.UIDispatcher.InvokeSafeAsync(() =>
            {
                // Get existing mod status
                var existing = GetModStatus(tuple);
                if (existing == null) return;

                // Update mod info
                existing.ModInfo = populatedModInfo;

                // Refresh collection view
                RefreshCollectionView();
            });
        }

        private async OrigTask PopulateInstalledModEntryAsync((SubmissionType, string, string fileId) tuple)
        {
            var populatedInstalledModEntry = await _taskFactory.StartNew(() => _persistenceService.GetInstalledMod(tuple.fileId));
            if (populatedInstalledModEntry == null) return;

            // Synchronize
            await DispatcherHelper.UIDispatcher.InvokeSafeAsync(() =>
            {
                // Get existing mod status
                var existing = GetModStatus(tuple);
                if (existing == null) return;

                // Update mod info
                existing.InstalledModEntry = populatedInstalledModEntry;
            });
        }

        private ModStatus AddModStatus((SubmissionType, string, string) tuple)
        {
            return DispatcherHelper.UIDispatcher.InvokeSafe(() =>
            {
                // Create stub status
                var modStatus = new ModStatus(tuple);

                // Add mod status
                Mods.Insert(0, modStatus);

                // Populate mod info from API in background
                PopulateModInfoAsync(tuple).Forget();

                // Populate installed mod entry in background
                PopulateInstalledModEntryAsync(tuple).Forget();

                return modStatus;
            });
        }

        private ModStatus GetModStatus((SubmissionType subType, string subId, string fileId) tuple, bool createIfMissing = false)
        {
            var result = Mods.FirstOrDefault(ms => ms?.ModInfo?.FileId == tuple.fileId);
            if (createIfMissing && result == null)
                result = AddModStatus(tuple);
            return result;
        }

        private void RemoveModStatus((SubmissionType, string, string) tuple)
        {
            DispatcherHelper.UIDispatcher.InvokeSafe(() =>
            {
                Mods.Remove(GetModStatus(tuple));
            });
        }

        /// <summary>
        /// Populates mod status for the currently executing task
        /// </summary>
        private void PopulateCurrent()
        {
            // Just in case
            if (_taskExecutionService.Task == null)
                return;

            // HACK: possible race maybe
            var task = _taskExecutionService.Task;
            var modStatus = GetModStatus(task.Identifier, true);
            if (modStatus == null) return;
            modStatus.State = ModStatusState.Processing;
            modStatus.StatusText = Localization.Localize(_taskExecutionService.Status);
            modStatus.StatusProgress = _taskExecutionService.Progress;
            modStatus.IsStatusProgressIndeterminate = _taskExecutionService.Progress <= 0;
            modStatus.IsStatusVisible = true;
            modStatus.IsAbortVisible = true;
            modStatus.IsReinstallVisible = false;
            modStatus.IsUninstallVisible = false;
        }

        /// <summary>
        /// Populates mod statuses for all currently enqueued mods to the list
        /// </summary>
        private void PopulateEnqueuedMods()
        {
            foreach (var task in _taskExecutionService.TaskQueue)
            {
                var modStatus = GetModStatus(task.Identifier, true);
                if (modStatus == null) return;
                modStatus.State = ModStatusState.InQueue;
                modStatus.StatusText = $"{Localization.Overview_InQueue} ({task.TaskType})";
                modStatus.IsStatusProgressIndeterminate = true;
                modStatus.IsStatusVisible = true;
                modStatus.IsAbortVisible = true;
                modStatus.IsReinstallVisible = false;
                modStatus.IsUninstallVisible = false;
            }
        }

        /// <summary>
        /// Populates mod status for a single installed mod
        /// </summary>
        private void PopulateInstalledMod((SubmissionType, string, string fileId) tuple)
        {
            var mod = _persistenceService.GetInstalledMod(tuple.fileId);
            if (mod == null) return;
            var modStatus = GetModStatus(tuple, true);
            if (modStatus == null) return;
            modStatus.State = ModStatusState.Idle;
            modStatus.IsStatusVisible = false;
            modStatus.IsAbortVisible = false;
            modStatus.IsReinstallVisible = true;
            modStatus.IsUninstallVisible = true;
        }

        /// <summary>
        /// Populates mod statuses for all installed mods to the list
        /// </summary>
        private void PopulateInstalledMods()
        {
            foreach (var mod in _persistenceService.GetInstalledMods())
            {
                var modStatus = GetModStatus(mod.Identifier, true);
                if (modStatus == null) return;
                modStatus.State = ModStatusState.Idle;
                modStatus.IsStatusVisible = false;
                modStatus.IsAbortVisible = false;
                modStatus.IsReinstallVisible = true;
                modStatus.IsUninstallVisible = true;
            }
        }

        /// <summary>
        /// Populate everything
        /// </summary>
        private void PopulateAll()
        {
            PopulateInstalledMods();
            PopulateEnqueuedMods();
            PopulateCurrent();
        }
    }
}