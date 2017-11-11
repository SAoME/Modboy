// ------------------------------------------------------------------ 
//  Solution: <GameBananaClient>
//  Project: <Modboy>
//  File: <Task.cs>
//  Created By: Alexey Golub
//  Date: 13/02/2016
// ------------------------------------------------------------------ 

namespace Modboy.Models.Internal
{
    public class Task
    {
        /// <summary>
        /// Type of this task
        /// </summary>
        public TaskType Type { get; }

        /// <summary>
        /// ID of the task target
        /// </summary>
        public string ModId { get; }

        public Task(TaskType type, string modId)
        {
            Type = type;
            ModId = modId;
        }
    }
}