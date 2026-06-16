using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal sealed class TaskEditForm : Form
    {
        private readonly TextBox _titleTextBox;
        private readonly FlowLayoutPanel _actionsPanel;
        private readonly Label _currentStatusLabel;
        private readonly Dictionary<TaskStatus, Button> _statusButtons;
        private readonly TaskItem _sourceTask;
        private TaskStatus _selectedStatus;

        public TaskEditForm(TaskItem task)
        {
            _sourceTask = task.Clone();
            _selectedStatus = _sourceTask.Status;
            _statusButtons = new Dictionary<TaskStatus, Button>();

            BackColor = Color.White;
            ClientSize = new Size(520, 360);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Редактирование задачи";

            var titleLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 38, 59),
                Location = new Point(24, 20),
                Text = "Редактирование"
            };

            var closeButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(111, 124, 140),
                Location = new Point(466, 18),
                Size = new Size(30, 30),
                Text = "✕",
                UseVisualStyleBackColor = false
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (sender, args) => Close();

            var statusCaption = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(111, 124, 140),
                Location = new Point(24, 70),
                Text = "Текущий статус"
            };

            _currentStatusLabel = new Label
            {
                AutoSize = false,
                BackColor = Color.FromArgb(243, 246, 251),
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 38, 59),
                Location = new Point(24, 96),
                Padding = new Padding(10, 6, 10, 6),
                Size = new Size(160, 34),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var actionCaption = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(111, 124, 140),
                Location = new Point(24, 146),
                Text = "Изменить статус"
            };

            _actionsPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                Location = new Point(24, 172),
                WrapContents = true
            };

            var textCaption = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(111, 124, 140),
                Location = new Point(24, 224),
                Text = "Текст задачи"
            };

            _titleTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                Location = new Point(24, 250),
                Size = new Size(472, 32),
                Text = _sourceTask.Title
            };

            var deleteButton = new Button
            {
                BackColor = Color.FromArgb(255, 240, 239),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(201, 69, 58),
                Location = new Point(24, 306),
                Size = new Size(146, 38),
                Text = "Удалить задачу",
                UseVisualStyleBackColor = false
            };
            deleteButton.FlatAppearance.BorderColor = Color.FromArgb(238, 193, 189);
            deleteButton.Click += DeleteButton_Click;

            var applyButton = new Button
            {
                BackColor = Color.FromArgb(56, 132, 255),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(350, 306),
                Size = new Size(146, 38),
                Text = "Применить",
                UseVisualStyleBackColor = false
            };
            applyButton.FlatAppearance.BorderSize = 0;
            applyButton.Click += ApplyButton_Click;

            Controls.Add(titleLabel);
            Controls.Add(closeButton);
            Controls.Add(statusCaption);
            Controls.Add(_currentStatusLabel);
            Controls.Add(actionCaption);
            Controls.Add(_actionsPanel);
            Controls.Add(textCaption);
            Controls.Add(_titleTextBox);
            Controls.Add(deleteButton);
            Controls.Add(applyButton);

            BuildStatusButtons();
            UpdateStatusUi();
        }

        public TaskItem EditedTask { get; private set; }

        public bool DeleteRequested { get; private set; }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            var title = (_titleTextBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show(this, "Введите текст задачи.", "Пустая задача", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EditedTask = _sourceTask.Clone();
            EditedTask.Title = title;
            EditedTask.Status = _selectedStatus;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            DeleteRequested = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BuildStatusButtons()
        {
            _actionsPanel.Controls.Clear();
            _statusButtons.Clear();

            foreach (var status in GetAvailableStatuses(_sourceTask.Status))
            {
                var button = new Button
                {
                    AutoSize = true,
                    BackColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(27, 38, 59),
                    Margin = new Padding(0, 0, 10, 10),
                    Padding = new Padding(10, 7, 10, 7),
                    Text = GetActionLabel(_sourceTask.Status, status),
                    UseVisualStyleBackColor = false,
                    Tag = status
                };
                button.FlatAppearance.BorderColor = Color.FromArgb(209, 219, 232);
                button.Click += StatusButton_Click;
                _actionsPanel.Controls.Add(button);
                _statusButtons[status] = button;
            }
        }

        private void StatusButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            _selectedStatus = (TaskStatus)button.Tag;
            UpdateStatusUi();
        }

        private void UpdateStatusUi()
        {
            _currentStatusLabel.Text = Form1.GetStatusText(_selectedStatus);

            foreach (var pair in _statusButtons)
            {
                var isSelected = pair.Key == _selectedStatus;
                pair.Value.BackColor = isSelected ? Color.FromArgb(56, 132, 255) : Color.White;
                pair.Value.ForeColor = isSelected ? Color.White : Color.FromArgb(27, 38, 59);
                pair.Value.FlatAppearance.BorderColor = isSelected ? Color.FromArgb(56, 132, 255) : Color.FromArgb(209, 219, 232);
            }
        }

        private static IEnumerable<TaskStatus> GetAvailableStatuses(TaskStatus currentStatus)
        {
            if (currentStatus == TaskStatus.Open)
            {
                yield return TaskStatus.InProgress;
                yield return TaskStatus.Closed;
                yield break;
            }

            if (currentStatus == TaskStatus.InProgress)
            {
                yield return TaskStatus.Open;
                yield return TaskStatus.Closed;
                yield break;
            }

            yield return TaskStatus.Open;
        }

        private static string GetActionLabel(TaskStatus currentStatus, TaskStatus nextStatus)
        {
            if (currentStatus == TaskStatus.InProgress && nextStatus == TaskStatus.Open)
            {
                return "Отложить";
            }

            if (currentStatus == TaskStatus.Closed && nextStatus == TaskStatus.Open)
            {
                return "Переоткрыть";
            }

            if (nextStatus == TaskStatus.InProgress)
            {
                return "В работу";
            }

            if (nextStatus == TaskStatus.Closed)
            {
                return "Закрыть";
            }

            return Form1.GetStatusText(nextStatus);
        }
    }
}
