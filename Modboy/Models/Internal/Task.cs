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
        public TaskType TaskType { get; }

        /// <summary>
        /// Type of the submission
        /// </summary>
        public SubmissionType SubmissionType { get; }

        /// <summary>
        /// Id of the submission
        /// </summary>
        public string SubmissionId { get; }

        /// <summary>
        /// Id of the file in the submission
        /// </summary>
        public string FileId { get; }

        public (SubmissionType submissionType, string submissionId, string fileId) Identifier => (submissionType: SubmissionType, submissionId: SubmissionId, fileId: FileId);

        public bool Matches((SubmissionType subType, string subId, string fileId) tuple)
        {
            return SubmissionType == tuple.subType && SubmissionId == tuple.subId && FileId == tuple.fileId;
        }

        public Task(TaskType type, SubmissionType subType,  string subId, string fileId)
        {
            TaskType = type;
            SubmissionType = subType;
            SubmissionId = subId;
            FileId = fileId;
        }

        public Task(TaskType type, (SubmissionType subType, string subId, string fileId) tuple)
        {
            TaskType = type;
            SubmissionType = tuple.subType;
            SubmissionId = tuple.subId;
            FileId = tuple.fileId;
        }
    }
}