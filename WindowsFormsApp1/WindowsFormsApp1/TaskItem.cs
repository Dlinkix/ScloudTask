using System;

namespace WindowsFormsApp1
{
    internal enum TaskStatus
    {
        Open = 0,
        InProgress = 1,
        Closed = 2
    }

    internal sealed class TaskItem
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public TaskStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public TaskItem Clone()
        {
            return new TaskItem
            {
                Id = Id,
                Title = Title,
                Status = Status,
                CreatedAt = CreatedAt
            };
        }
    }
}
