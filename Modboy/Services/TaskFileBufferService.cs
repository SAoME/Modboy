﻿// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <TaskFileBufferService.cs>
//  Created By: Alexey Golub
//  Date: 13/02/2016
// ------------------------------------------------------------------ 

using System.Collections.Generic;
using NegativeLayer.Extensions;
using System.IO;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using Modboy.Models.Internal;
using Tyrrrz.WpfExtensions;
using System;

namespace Modboy.Services
{
    public class TaskFileBufferService
    {
        private readonly TaskExecutionService _taskExecutionService;
        private readonly WindowService _windowService;

        private readonly FileSystemWatcher _watcher;

        public TaskFileBufferService(TaskExecutionService taskExecutionService, WindowService windowService)
        {
            _taskExecutionService = taskExecutionService;
            _windowService = windowService;

            // Set up watcher
            string dir = Path.GetDirectoryName(FileSystem.TaskBufferFilePath);
            string file = Path.GetFileName(FileSystem.TaskBufferFilePath);
            _watcher = new FileSystemWatcher(dir, file)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName
            };

            // Events
            _watcher.Changed += (sender, args) => ParseBufferFile();
            DispatcherHelper.UIDispatcher.InvokeSafeAsync(() => Application.Current.Exit += (sender, args) => DumpQueue()).Forget();
        }

        /// <summary>
        /// Parse the buffer file, enqueue all tasks it has and delete it
        /// </summary>
        private void ParseBufferFile()
        {
            // If the file doesn't exist - return
            if (!File.Exists(FileSystem.TaskBufferFilePath))
                return;

            // Silence the watcher temporarily
            _watcher.EnableRaisingEvents = false;

            // Get the data
            var bufferLines = File.ReadAllLines(FileSystem.TaskBufferFilePath);

            // Enqueue everything
            foreach (string bufferLine in bufferLines.Distinct())
            {
                var parts = bufferLine.SplitTrim(Constants.UniformSeparator);
                var taskType = parts[0];
                var subType = parts[1];
                var subId = parts[2];
                var fileId = parts[3];

                SubmissionType parsedSubType;
                try
                {
                    parsedSubType = subType.ParseEnum<SubmissionType>();
                }
                catch (ArgumentException ex)
                {
                    // Resume the watcher
                    _watcher.EnableRaisingEvents = true;
                    _windowService.ShowErrorWindowAsync(Localization.Current.Task_UnknownInstallationProcess).GetResult();
                    return;
                }
                
                _taskExecutionService.EnqueueTask(
                    new Task(
                        taskType.ParseEnum<TaskType>(),
                        parsedSubType,
                        subId,
                        fileId
                    )
                );
            }

            // Clear file
            File.WriteAllText(FileSystem.TaskBufferFilePath, string.Empty);

            // Resume the watcher
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Dumps the current queue back to the file
        /// </summary>
        private void DumpQueue()
        {
            // Silence the watcher temporarily
            _watcher.EnableRaisingEvents = false;

            // Get the queued objects
            var queue = new List<Task>();
            if (_taskExecutionService.Task != null)
                queue.Add(_taskExecutionService.Task);
            queue.AddRange(_taskExecutionService.TaskQueue);

            // Select the string representations of queued tasks
            var bufferLines = queue
                .Select(t =>
                    string.Join(
                        Constants.UniformSeparator,
                        t.TaskType.ToString(),
                        t.SubmissionType,
                        t.SubmissionId,
                        t.FileId
                    )
                )
                .ToArray();

            // Dump to file
            File.WriteAllLines(FileSystem.TaskBufferFilePath, bufferLines);

            // Resume the watcher
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Start monitoring the buffer file
        /// </summary>
        public void Start()
        {
            _watcher.EnableRaisingEvents = true;

            // Proc first event
            ParseBufferFile();
        }

        /// <summary>
        /// Stop monitoring the buffer file
        /// </summary>
        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
        }
    }
}