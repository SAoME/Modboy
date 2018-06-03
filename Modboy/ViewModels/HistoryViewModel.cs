// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <HistoryViewModel.cs>
//  Created By: Alexey Golub
//  Date: 06/03/2016
// ------------------------------------------------------------------ 

using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using Modboy.Models.Internal;
using Modboy.Services;
using NegativeLayer.Extensions;
using Tyrrrz.WpfExtensions;

namespace Modboy.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        public class HistoryViewEntry
        {
            public DateTime Date { get; }
			public TaskType TaskType { get; }
            public string EventDescription { get; }
			public string ModUrl { get; }

            public HistoryViewEntry(DateTime date, TaskType taskType, string eventDescription, string modUrl)
            {
                Date = date;
				TaskType = taskType;
                EventDescription = eventDescription;
				ModUrl = modUrl;
            }
        }

        private readonly HistoryService _historyService;

		public string[] TaskTypes { get; } = typeof(TaskType).GetEnumNames();

		private string _taskTypeFilter;
		public string TaskTypeFilter
		{
			get => _taskTypeFilter;
			set
			{
				Set(ref _taskTypeFilter, value);
				PopulateHistory();
			}
		}

        public Localization Localization { get; } = Localization.Current;

        // History entries
        public ObservableCollection<HistoryViewEntry> History { get; } = new ObservableCollection<HistoryViewEntry>();

        // Commands
        public RelayCommand ClearHistoryCommand { get; }
		public RelayCommand ClearFilters { get; }

        public HistoryViewModel(HistoryService historyService)
        {
            _historyService = historyService;

            // Commands
            ClearHistoryCommand = new RelayCommand(() =>
            {
                _historyService.ClearHistory();
                PopulateHistory();
            });
			ClearFilters = new RelayCommand(
				() => TaskTypeFilter = null,
				() => TaskTypeFilter.IsNotBlank()
			);

            // Initial population
            PopulateHistory();

            // Events
            Localization.PropertyChanged += (sender, args) => PopulateHistory();
            _historyService.HistoryEntryRecorded += (sender, args) =>
            {
				if (TaskTypeFilter.IsBlank() || args.Entry.TaskType.ToString().Equals(TaskTypeFilter))
				{
					DispatcherHelper.UIDispatcher.InvokeSafe(
						() => History.Insert(0, new HistoryViewEntry(args.Entry.Date, args.Entry.TaskType, Localization.Localize(args.Entry), args.Entry.ModUrl)));
				}
            };
        }

        private void PopulateHistory()
        {
            History.Clear();
			var historyFiltered = TaskTypeFilter.IsBlank() ? _historyService.GetHistory() : _historyService.GetHistoryByTaskType((TaskType) Enum.Parse(typeof(TaskType), TaskTypeFilter));
			foreach (var entry in historyFiltered)
			{
				History.Add(new HistoryViewEntry(entry.Date, entry.TaskType, Localization.Localize(entry), entry.ModUrl));
			}
			ClearFilters.RaiseCanExecuteChanged();
		}
    }
}