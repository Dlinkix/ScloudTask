using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal static class TaskStorage
    {
        private static readonly string FilePath = Path.Combine(Application.StartupPath, "tasks.db");

        public static List<TaskItem> Load()
        {
            if (!File.Exists(FilePath))
            {
                return CreateDemoTasks();
            }

            var tasks = new List<TaskItem>();
            foreach (var line in File.ReadAllLines(FilePath, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split('|');
                if (parts.Length != 4)
                {
                    continue;
                }

                Guid id;
                long ticks;
                int statusValue;

                if (!Guid.TryParse(parts[0], out id)
                    || !long.TryParse(parts[1], out ticks)
                    || !int.TryParse(parts[2], out statusValue))
                {
                    continue;
                }

                string title;
                try
                {
                    title = Encoding.UTF8.GetString(Convert.FromBase64String(parts[3]));
                }
                catch
                {
                    continue;
                }

                tasks.Add(new TaskItem
                {
                    Id = id,
                    CreatedAt = new DateTime(ticks),
                    Status = (TaskStatus)statusValue,
                    Title = title
                });
            }

            return tasks.Count > 0 ? tasks : CreateDemoTasks();
        }

        public static void Save(IEnumerable<TaskItem> tasks)
        {
            var lines = tasks.Select(task =>
                string.Join("|",
                    task.Id,
                    task.CreatedAt.Ticks,
                    (int)task.Status,
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(task.Title ?? string.Empty))));

            File.WriteAllLines(FilePath, lines, Encoding.UTF8);
        }

        private static List<TaskItem> CreateDemoTasks()
        {
            var now = DateTime.Now;
            return new List<TaskItem>
            {
                new TaskItem { Id = Guid.NewGuid(), Title = "Подготовить структуру Scloud", Status = TaskStatus.Open, CreatedAt = now.AddMinutes(-30) },
                new TaskItem { Id = Guid.NewGuid(), Title = "Собрать проект", Status = TaskStatus.InProgress, CreatedAt = now.AddMinutes(-20) },
                new TaskItem { Id = Guid.NewGuid(), Title = "Проверить сценарий редактирования", Status = TaskStatus.Closed, CreatedAt = now.AddMinutes(-10) }
            };
        }
    }
}
