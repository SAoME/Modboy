// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <TaskExecutionService.cs>
//  Created By: Alexey Golub
//  Date: 13/02/2016
// ------------------------------------------------------------------ 

using NegativeLayer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Modboy.Models.EventArgs;
using Modboy.Models.Internal;
using Task = Modboy.Models.Internal.Task;
using System.Net;
using Modboy.Models.API;

namespace Modboy.Services
{
    public class TaskExecutionService
    {
        private readonly PersistenceService _persistenceService;
        private readonly BackupService _backupService;
        private readonly AliasService _aliasService;
        private readonly CommandExecutionService _commandExecutionService;
        private readonly WindowService _windowService;
        private readonly ArchivingService _archivingService = new ArchivingService();
        private readonly WebService _webService = new WebService();
        private readonly APIService _apiService = new APIService();

        private readonly List<Task> _taskQueue = new List<Task>();
        private readonly TaskFactory _taskFactory = new TaskFactory(TaskCreationOptions.PreferFairness,
            TaskContinuationOptions.PreferFairness);
        private readonly List<string> _fileChanges = new List<string>();

        private bool _isEnabled;

        /// <summary>
        /// Task queue
        /// </summary>
        public IReadOnlyList<Task> TaskQueue
        {
            get
            {
                lock (_taskQueue)
                    return _taskQueue.AsReadOnly();
            }
        }

        /// <summary>
        /// Task, currently ran by the service
        /// </summary>
        public Task Task { get; private set; }

        /// <summary>
        /// Whether the task is currently being aborted
        /// </summary>
        public bool IsAbortPending { get; private set; }

        /// <summary>
        /// Status descriptor of the current operation
        /// </summary>
        public TaskExecutionStatus Status { get; private set; }

        /// <summary>
        /// Progress of the current operation
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// Event that triggers when a new task has been enqueed
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskAddedToQueue;

        /// <summary>
        /// Event that triggers when the task execution has just started
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskStarted;

        /// <summary>
        /// Event that triggers when the currently executing task was aborted and now stopped executing
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskAborted;

        /// <summary>
        /// Event that triggers when a task, that was not currently executing, was removed from the queue
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskRemovedFromQueue;

        /// <summary>
        /// Event that triggers when the task execution has ended
        /// </summary>
        public event EventHandler<TaskEndedEventArgs> TaskEnded;

        /// <summary>
        /// Event that triggers when the task execution has new status, new substatus or changed progress value
        /// </summary>
        public event EventHandler<TaskStateChangedEventArgs> TaskStateChanged;

        public TaskExecutionService(PersistenceService persistenceService, BackupService backupService,
            AliasService aliasService, CommandExecutionService commandExecutionService, WindowService windowService)
        {
            _persistenceService = persistenceService;
            _backupService = backupService;
            _aliasService = aliasService;
            _commandExecutionService = commandExecutionService;
            _windowService = windowService;

            // Delegates
            _webService.AbortChecker += () => IsAbortPending;
            _archivingService.AbortChecker += () => IsAbortPending;
            _apiService.AbortChecker += () => IsAbortPending;

            // Events
            _commandExecutionService.FileChangeMade += (sender, args) => _fileChanges.Add(args.FilePath);
            _webService.ProgressChanged += (sender, args) => UpdateStatus(args.Progress);
            _archivingService.ProgressChanged += (sender, args) => UpdateStatus(args.Progress);
        }

        /// <summary>
        /// Checks the queue for new tasks and pops the next available task
        /// </summary>
        private void PopQueue()
        {
            lock (_taskQueue)
            {
                Task = null;

                if (!_taskQueue.Any())
                    return;

                Task = _taskQueue[0];
                _taskQueue.RemoveAt(0);
            }

            // Start a thread to execute the task
            _taskFactory.StartNew(() =>
            {
                // Reset values
                IsAbortPending = false;
                TaskStarted?.Invoke(this, new TaskEventArgs(Task));
                ResetStatus();
                Logger.Record($"Executing task {Task.TaskType} for {Task.SubmissionType} {Task.SubmissionId}, file: {Task.FileId}");

                // Execute task
                bool result = ExecuteTask();

                // Reset values
                ResetStatus();
                Logger.Record($"Task executed {(result ? "successfully" : "unsuccessfully")}");
                TaskEnded?.Invoke(this, new TaskEndedEventArgs(Task, result));

                // Pop queue again
                PopQueue();
            });
        }

        /// <summary>
        /// Pops queue if a task isn't already executing
        /// </summary>
        public void TryPopQueue()
        {
            lock (_taskQueue)
            {
                if (Task == null)
                    PopQueue();
            }
        }

        private void UpdateStatus(TaskExecutionStatus status, double progress = 0)
        {
            Status = status;
            Progress = progress;
            TaskStateChanged?.Invoke(this, new TaskStateChangedEventArgs(Task, Status, Progress));
        }

        private void UpdateStatus(double progress = 0)
        {
            Progress = progress;
            TaskStateChanged?.Invoke(this, new TaskStateChangedEventArgs(Task, Status, Progress));
        }

        private void ResetStatus()
        {
            UpdateStatus(TaskExecutionStatus.None);
        }

        private ModInstallationState ExecuteVerifyTask()
        {
            UpdateStatus(TaskExecutionStatus.Verify);

            // Get files affected by install
            UpdateStatus(TaskExecutionStatus.VerifyGetAffectedFiles);
            var affectedFiles = _persistenceService.GetAffectedFiles(Task.FileId);
            if (!affectedFiles.AnySafe()) return ModInstallationState.NotInstalled;

            // Normalize affected files
            if (affectedFiles.Length > 1)
                affectedFiles = affectedFiles.ToArray();
            affectedFiles = affectedFiles.Select(f => f.Replace("\\", "/")).ToArray();
            
            // Verify files
            UpdateStatus(TaskExecutionStatus.VerifyExecute);
            foreach (var file in affectedFiles)
            {
                if (IsAbortPending)
                {
                    TaskAborted?.Invoke(this, new TaskEventArgs(Task));
                    return ModInstallationState.Unknown;
                }

                if (file.IsBlank())
                    return ModInstallationState.Corrupt;

                // Check if exists
                if (!File.Exists(file))
                    return ModInstallationState.Corrupt;

                UpdateStatus(Progress + 100.0/affectedFiles.Length);
            }

            // All checks passed => installed
            return ModInstallationState.Installed;
        }

        private bool ExecuteInstallTask()
        {
            UpdateStatus(TaskExecutionStatus.Install);

            // Get mod info
            UpdateStatus(TaskExecutionStatus.InstallGetModInfo);
            var modInfo = _apiService.GetModInfo(Task.Identifier);
            if (modInfo == null) return false;

            // Reset everything
            _fileChanges.Clear();

            // Retrieve file info
            var fileInfo = _apiService.GetFileInfo(modInfo.FileId);

            // Download archive
            UpdateStatus(TaskExecutionStatus.InstallDownload);
            var downloadedFileContainer = _webService.Download(fileInfo.DownloadUrl, FileSystem.CreateTempFile($"Mod_{modInfo.FileId}"));
            if (downloadedFileContainer == null)
            {
                return false;
            }
            var contentType = downloadedFileContainer.ResponseHeaders[HttpResponseHeader.ContentType];
            var downloadedFile = downloadedFileContainer.FileInfo;
            if (IsAbortPending)
            {
                TaskAborted?.Invoke(this, new TaskEventArgs(Task));
                return false;
            }

            // Unpack archive
            UpdateStatus(TaskExecutionStatus.InstallUnpack);
            string unpackedDir = FileSystem.CreateTempDirectory($"Mod_{modInfo.FileId}");
            _aliasService.Set(new InternalAlias(InternalAliasKeyword.ArchiveExtractedDirectory, unpackedDir));
            var extractSuccess = _archivingService.ExtractFiles(downloadedFile.FullName, unpackedDir);
            if (!extractSuccess || !Directory.Exists(unpackedDir))
            {
				// Note: RAR5 supported now, but might be useful for future formats
                //if ("application/x-rar-compressed".Equals(contentType))
                //{
                //    _windowService.ShowErrorWindowAsync(Localization.Current.Task_Install_Unpack_Failed_RAR5).GetResult();
                //}
                //else
                //{
                _windowService.ShowErrorWindowAsync(Localization.Current.Task_Install_Unpack_Failed).GetResult();
                //}
                return false;
            }
            if (IsAbortPending)
            {
                TaskAborted?.Invoke(this, new TaskEventArgs(Task));
                return false;
            }

            // Get commands
            UpdateStatus(TaskExecutionStatus.InstallExecute);
            var commands = fileInfo.InstallationCommands;
            string commandContextID = modInfo.FileId; // can be improved later

            // If we aren't copying files, we probably don't have an installation scheme for this type of submission
            if (!commands.Any(c => CommandType.Copy.Equals(c.Type)))
            {
                _windowService.ShowErrorWindowAsync(Localization.Current.Task_UnknownInstallationProcess).GetResult();
                return false;
            }

            // Execute commands
            foreach (var command in commands)
            {
                if (IsAbortPending)
                {
                    TaskAborted?.Invoke(this, new TaskEventArgs(Task));
                    return false;
                }

                bool success = _commandExecutionService.ExecuteCommand(command, commandContextID);
                if (!success)
                {
#if !DEBUG
                    _windowService.ShowErrorWindowAsync(Localization.Current.Task_CommandExecutionFailed).GetResult();
                    return false;
#endif
                }

                UpdateStatus(Progress + 100.0/commands.Count);
            }

            // Clear local aliases
            _aliasService.Clear(commandContextID);

            // Store results
            UpdateStatus(TaskExecutionStatus.InstallStoreResults);
            _persistenceService.RecordInstall(modInfo.Identifier, _fileChanges.ToArray());

            return true;
        }

        private bool ExecuteUninstallTask()
        {
            UpdateStatus(TaskExecutionStatus.Uninstall);

            // Get files, affected by install
            UpdateStatus(TaskExecutionStatus.UninstallGetAffectedFiles);
            var affectedFiles = _persistenceService.GetAffectedFiles(Task.FileId);
            if (!affectedFiles.AnySafe()) return true;

            // Delete files
            UpdateStatus(TaskExecutionStatus.UninstallDeleteFiles);
            foreach (string file in affectedFiles)
            {
                if (IsAbortPending)
                {
                    TaskAborted?.Invoke(this, new TaskEventArgs(Task));
                    return false;
                }

                if (File.Exists(file))
                    File.Delete(file);

                UpdateStatus(Progress + 100.0/affectedFiles.Length);
            }

            // Restore backups
            UpdateStatus(TaskExecutionStatus.UninstallRestoreBackups);
            _backupService.RestoreAll(Task.FileId);

            // Store results
            UpdateStatus(TaskExecutionStatus.UninstallStoreResults);
            _persistenceService.RecordUninstall(Task.Identifier);

            return true;
        }

        private bool ExecuteTask()
        {
            // Install
            if (Task.TaskType == TaskType.Install)
            {
                // Verify
                var integrity = ExecuteVerifyTask();
                
                // Prompt user
                if (integrity.IsEither(ModInstallationState.Installed, ModInstallationState.Corrupt) &&
                    !_windowService.ShowPromptWindowAsync(Localization.Current.Task_PromptReinstall).GetResult())
                    return false;

                // Clean install
                bool installSuccess = false;
                if (integrity == ModInstallationState.NotInstalled)
                {
                    // Just install
                    installSuccess = ExecuteInstallTask();
                }
                // Dirty install
                else
                {
                    // Try to uninstall previous version
                    bool uninstallSuccess = ExecuteUninstallTask();
                    // If successful - install new version
                    if (uninstallSuccess)
                        installSuccess = ExecuteInstallTask();
                }

                // If failed to install - uninstall
                if (!installSuccess)
                    ExecuteUninstallTask();

                return installSuccess;
            }

            // Uninstall
            if (Task.TaskType == TaskType.Uninstall)
            {
                // Check if installed
                if (!_persistenceService.IsInstalled(Task.FileId))
                    return true;

                // Prompt user
                if (!_windowService.ShowPromptWindowAsync(Localization.Current.Task_PromptUninstall).GetResult())
                    return false;

                // Execute uninstall
                return ExecuteUninstallTask();
            }

            // Verify
            if (Task.TaskType == TaskType.Verify)
            {
                // Verify
                var integrity = ExecuteVerifyTask();

                // If not installed - remove trails
                if (integrity == ModInstallationState.NotInstalled)
                    ExecuteUninstallTask();

                return ExecuteVerifyTask() == ModInstallationState.Installed;
            }

            // Unknown
            return false;
        }

        /// <summary>
        /// Enqueues a task for execution
        /// </summary>
        public void EnqueueTask(Task task)
        {
            // Enqueue
            lock (_taskQueue)
            {
                _taskQueue.Add(task);
                TaskAddedToQueue?.Invoke(this, new TaskEventArgs(task));
            }

            // If not busy - pop queue straight away
            TryPopQueue();
        }

        /// <summary>
        /// Removes task from queue
        /// </summary>
        public void RemoveTaskFromQueue(Task task)
        {
            lock (_taskQueue)
            {
                if (_taskQueue.Remove(task))
                    TaskRemovedFromQueue?.Invoke(this, new TaskEventArgs(task));
            }
        }

        /// <summary>
        /// Aborts the execution of the current task as soon as possible
        /// </summary>
        public void AbortCurrentTask()
        {
            IsAbortPending = true;
            Logger.Record("Issued abort for current task");
        }

        /// <summary>
        /// Aborts the execution of the given task if it's running or removes it from the queue if it's enqueued
        /// </summary>
        public void AbortTask(Task task)
        {
            lock (_taskQueue)
            {
                // Current task
                if (Task == task)
                    AbortCurrentTask();

                // Tasks in queue
                RemoveTaskFromQueue(task);
            }
        }

        /// <summary>
        /// Aborts task by mod id
        /// </summary>
        public void AbortTask((SubmissionType subType, string subId, string fileId) tuple)
        {
            lock (_taskQueue)
            {
                // Current task
                if (Task != null && Task.Matches(tuple))
                {
                    AbortCurrentTask();
                }

                // Tasks in queue
                var enqueuedTasks = _taskQueue.Where(t => t.Matches(tuple)).ToArray();
                foreach (var task in enqueuedTasks)
                {
                    _taskQueue.Remove(task);
                    TaskRemovedFromQueue?.Invoke(this, new TaskEventArgs(task));
                }
            }
        }

        /// <summary>
        /// Aborts the execution of the current task as soon as possible and clears the task queue
        /// </summary>
        public void AbortAllTasks()
        {
            lock (_taskQueue)
            {
                AbortCurrentTask();
                _taskQueue.ForEach(RemoveTaskFromQueue);
            }
        }
    }
}