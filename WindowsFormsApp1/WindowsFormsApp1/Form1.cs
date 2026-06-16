using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private readonly List<TaskItem> _tasks;
        private readonly Dictionary<TaskStatus, Label> _summaryCountLabels;
        private readonly Dictionary<TaskStatus, FlowLayoutPanel> _boardColumns;
        private readonly FlowLayoutPanel _contentPanel;
        private readonly FlowLayoutPanel _summaryCardsPanel;
        private readonly Panel _summarySection;
        private readonly Panel _addSection;
        private readonly Panel _tasksSection;
        private readonly Panel _boardSection;
        private readonly FlowLayoutPanel _tasksListPanel;
        private readonly TableLayoutPanel _boardTable;
        private readonly TextBox _newTaskTextBox;
        private readonly Button _clearTaskButton;
        private readonly Button _toggleTasksButton;
        private bool _showAllTasks;
        private bool _inputHovered;

        public Form1()
        {
            InitializeComponent();

            _tasks = TaskStorage.Load();
            _summaryCountLabels = new Dictionary<TaskStatus, Label>();
            _boardColumns = new Dictionary<TaskStatus, FlowLayoutPanel>();

            BackColor = Color.FromArgb(244, 247, 252);
            MinimumSize = new Size(360, 640);
            Text = "ToDo List Scloud";
            StartPosition = FormStartPosition.CenterScreen;

            _contentPanel = new FlowLayoutPanel
            {
                AutoScroll = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(244, 247, 252)
            };

            _summarySection = CreateSectionPanel();
            _summaryCardsPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Location = new Point(0, 48),
                WrapContents = true
            };

            _addSection = CreateSectionPanel();
            _tasksSection = CreateSectionPanel();
            _tasksListPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                Location = new Point(0, 48),
                WrapContents = false
            };

            _toggleTasksButton = new Button
            {
                AutoSize = false,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 132, 255),
                Size = new Size(140, 38),
                Text = "Показать еще",
                UseVisualStyleBackColor = false
            };
            _toggleTasksButton.FlatAppearance.BorderColor = Color.FromArgb(208, 221, 240);
            _toggleTasksButton.Click += ToggleTasksButton_Click;

            _boardSection = CreateSectionPanel();
            _boardTable = new TableLayoutPanel
            {
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                ColumnCount = 3,
                Location = new Point(0, 48),
                Margin = new Padding(0),
                RowCount = 1
            };

            _newTaskTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                Text = string.Empty
            };
            _newTaskTextBox.TextChanged += NewTaskTextBox_TextChanged;
            _newTaskTextBox.MouseEnter += (sender, args) => { _inputHovered = true; UpdateClearButtonVisibility(); };
            _newTaskTextBox.MouseLeave += (sender, args) => { _inputHovered = false; UpdateClearButtonVisibility(); };
            _newTaskTextBox.GotFocus += (sender, args) => UpdateClearButtonVisibility();
            _newTaskTextBox.LostFocus += (sender, args) => UpdateClearButtonVisibility();
            _newTaskTextBox.KeyDown += NewTaskTextBox_KeyDown;

            _clearTaskButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(111, 124, 140),
                Size = new Size(28, 28),
                Text = "✕",
                UseVisualStyleBackColor = false
            };
            _clearTaskButton.FlatAppearance.BorderSize = 0;
            _clearTaskButton.MouseEnter += (sender, args) => { _inputHovered = true; UpdateClearButtonVisibility(); };
            _clearTaskButton.MouseLeave += (sender, args) => { _inputHovered = false; UpdateClearButtonVisibility(); };
            _clearTaskButton.Click += (sender, args) =>
            {
                _newTaskTextBox.Clear();
                _newTaskTextBox.Focus();
            };

            Controls.Add(_contentPanel);

            BuildSummarySection();
            BuildAddSection();
            BuildTasksSection();
            BuildBoardSection();

            _contentPanel.Controls.Add(_summarySection);
            _contentPanel.Controls.Add(_addSection);
            _contentPanel.Controls.Add(_tasksSection);
            _contentPanel.Controls.Add(_boardSection);

            Resize += (sender, args) => ApplyResponsiveLayout();
            Shown += (sender, args) => ApplyResponsiveLayout();

            RenderAll();
        }

        internal static string GetStatusText(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Open:
                    return "Открыт";
                case TaskStatus.InProgress:
                    return "В работе";
                case TaskStatus.Closed:
                    return "Закрыт";
                default:
                    return string.Empty;
            }
        }

        private void BuildSummarySection()
        {
            AddSectionTitle(_summarySection, "Текущие задачи");

            foreach (var status in new[] { TaskStatus.Open, TaskStatus.InProgress, TaskStatus.Closed })
            {
                var card = new Panel
                {
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 16, 16),
                    Padding = new Padding(18),
                    Size = new Size(220, 108)
                };

                var titleLabel = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(111, 124, 140),
                    Location = new Point(18, 18),
                    Text = GetStatusText(status)
                };

                var countLabel = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI Semibold", 28F, FontStyle.Bold),
                    ForeColor = GetStatusColor(status),
                    Location = new Point(18, 42),
                    Text = "0"
                };

                card.Controls.Add(titleLabel);
                card.Controls.Add(countLabel);
                _summaryCountLabels[status] = countLabel;
                _summaryCardsPanel.Controls.Add(card);
            }

            _summarySection.Controls.Add(_summaryCardsPanel);
        }

        private void BuildAddSection()
        {
            AddSectionTitle(_addSection, "Добавление новых задач");

            var inputCard = new Panel
            {
                BackColor = Color.White,
                Height = 64,
                Location = new Point(0, 48)
            };
            inputCard.MouseEnter += (sender, args) => { _inputHovered = true; UpdateClearButtonVisibility(); };
            inputCard.MouseLeave += (sender, args) => { _inputHovered = false; UpdateClearButtonVisibility(); };

            var addButton = new Button
            {
                BackColor = Color.FromArgb(56, 132, 255),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 12),
                Size = new Size(40, 40),
                Text = "+",
                UseVisualStyleBackColor = false
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (sender, args) => AddTask();

            _newTaskTextBox.Location = new Point(72, 21);
            _newTaskTextBox.Width = 520;

            _clearTaskButton.Location = new Point(610, 18);

            inputCard.Controls.Add(addButton);
            inputCard.Controls.Add(_newTaskTextBox);
            inputCard.Controls.Add(_clearTaskButton);
            _addSection.Controls.Add(inputCard);
            _addSection.Tag = inputCard;
        }

        private void BuildTasksSection()
        {
            AddSectionTitle(_tasksSection, "Задачи");
            _tasksSection.Controls.Add(_tasksListPanel);
            _tasksSection.Controls.Add(_toggleTasksButton);
        }

        private void BuildBoardSection()
        {
            AddSectionTitle(_boardSection, "Доска задач");
            _boardTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            foreach (var status in new[] { TaskStatus.Open, TaskStatus.InProgress, TaskStatus.Closed })
            {
                _boardTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));

                var columnCard = new Panel
                {
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    Padding = new Padding(16),
                    MinimumSize = new Size(200, 300)
                };

                var title = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(27, 38, 59),
                    Location = new Point(16, 16),
                    Text = GetStatusText(status)
                };

                var flow = new FlowLayoutPanel
                {
                    AllowDrop = true,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown,
                    Location = new Point(16, 52),
                    WrapContents = false,
                    Tag = status
                };
                flow.DragEnter += BoardColumn_DragEnter;
                flow.DragDrop += BoardColumn_DragDrop;

                columnCard.Controls.Add(title);
                columnCard.Controls.Add(flow);
                _boardColumns[status] = flow;
                _boardTable.Controls.Add(columnCard);
            }

            _boardSection.Controls.Add(_boardTable);
        }

        private void AddTask()
        {
            var title = (_newTaskTextBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show(this, "Введите текст задачи перед добавлением.", "Новая задача", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _tasks.Add(new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = title,
                Status = TaskStatus.Open,
                CreatedAt = DateTime.Now
            });

            _newTaskTextBox.Clear();
            SaveAndRender();
        }

        private void EditTask(Guid taskId)
        {
            var task = _tasks.FirstOrDefault(item => item.Id == taskId);
            if (task == null)
            {
                return;
            }

            using (var dialog = new TaskEditForm(task))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                if (dialog.DeleteRequested)
                {
                    _tasks.Remove(task);
                    SaveAndRender();
                    return;
                }

                if (dialog.EditedTask == null)
                {
                    return;
                }

                task.Title = dialog.EditedTask.Title;
                task.Status = dialog.EditedTask.Status;
                SaveAndRender();
            }
        }

        private void SaveAndRender()
        {
            TaskStorage.Save(_tasks);
            RenderAll();
        }

        private void RenderAll()
        {
            RenderSummary();
            RenderTaskRows();
            RenderBoard();
            ApplyResponsiveLayout();
        }

        private void RenderSummary()
        {
            foreach (var status in _summaryCountLabels.Keys.ToList())
            {
                _summaryCountLabels[status].Text = _tasks.Count(task => task.Status == status).ToString();
            }
        }

        private void RenderTaskRows()
        {
            _tasksListPanel.SuspendLayout();
            _tasksListPanel.Controls.Clear();

            var orderedTasks = GetOrderedTasks().ToList();
            var visibleTasks = _showAllTasks ? orderedTasks : orderedTasks.Take(5);

            foreach (var task in visibleTasks)
            {
                _tasksListPanel.Controls.Add(CreateTaskRow(task));
            }

            _tasksListPanel.ResumeLayout();

            _toggleTasksButton.Visible = orderedTasks.Count > 5;
            _toggleTasksButton.Text = _showAllTasks ? "Скрыть" : "Показать еще";
        }

        private void RenderBoard()
        {
            foreach (var panel in _boardColumns.Values)
            {
                panel.SuspendLayout();
                panel.Controls.Clear();
            }

            foreach (var task in GetOrderedTasks())
            {
                _boardColumns[task.Status].Controls.Add(CreateTaskCard(task));
            }

            foreach (var panel in _boardColumns.Values)
            {
                panel.ResumeLayout();
            }
        }

        private IEnumerable<TaskItem> GetOrderedTasks()
        {
            return _tasks
                .OrderBy(task => GetStatusOrder(task.Status))
                .ThenBy(task => task.CreatedAt);
        }

        private Control CreateTaskRow(TaskItem task)
        {
            var row = new Panel
            {
                BackColor = Color.White,
                Height = 72,
                Margin = new Padding(0, 0, 0, 12),
                Tag = task.Id
            };

            var titleLabel = new Label
            {
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(27, 38, 59),
                Location = new Point(18, 24),
                Size = new Size(520, 24),
                Text = task.Title
            };

            var statusButton = new Button
            {
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = GetStatusColor(task.Status),
                Size = new Size(140, 36),
                Tag = task.Id,
                Text = GetStatusText(task.Status),
                UseVisualStyleBackColor = false
            };
            statusButton.FlatAppearance.BorderColor = GetStatusColor(task.Status);
            statusButton.Click += TaskStatusButton_Click;

            row.Controls.Add(titleLabel);
            row.Controls.Add(statusButton);

            row.Resize += (sender, args) =>
            {
                titleLabel.Width = Math.Max(150, row.Width - 200);
                statusButton.Location = new Point(row.Width - statusButton.Width - 18, 18);
            };

            row.Click += (sender, args) => EditTask(task.Id);
            titleLabel.Click += (sender, args) => EditTask(task.Id);

            return row;
        }

        private Control CreateTaskCard(TaskItem task)
        {
            var card = new Panel
            {
                BackColor = Color.FromArgb(247, 250, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.SizeAll,
                Height = 86,
                Margin = new Padding(0, 0, 0, 12),
                Padding = new Padding(12),
                Tag = task.Id
            };

            var title = new Label
            {
                AutoEllipsis = true,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 38, 59),
                Location = new Point(12, 12),
                Size = new Size(200, 36),
                Text = task.Title
            };

            var badge = new Label
            {
                AutoSize = false,
                BackColor = GetStatusLightColor(task.Status),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = GetStatusColor(task.Status),
                Location = new Point(12, 52),
                Size = new Size(110, 22),
                Text = GetStatusText(task.Status),
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.Add(title);
            card.Controls.Add(badge);

            card.Resize += (sender, args) => title.Width = Math.Max(120, card.Width - 24);

            card.MouseDown += TaskCard_MouseDown;
            title.MouseDown += (sender, args) => TaskCard_MouseDown(card, args);
            badge.MouseDown += (sender, args) => TaskCard_MouseDown(card, args);
            card.DoubleClick += (sender, args) => EditTask(task.Id);
            title.DoubleClick += (sender, args) => EditTask(task.Id);

            return card;
        }

        private void ToggleTasksButton_Click(object sender, EventArgs e)
        {
            _showAllTasks = !_showAllTasks;
            RenderTaskRows();
            ApplyResponsiveLayout();

            if (!_showAllTasks)
            {
                _contentPanel.ScrollControlIntoView(_tasksSection);
            }
        }

        private void TaskStatusButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            EditTask((Guid)button.Tag);
        }

        private void TaskCard_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            var control = sender as Control;
            if (control == null)
            {
                return;
            }

            var taskId = (Guid)control.Tag;
            control.DoDragDrop(taskId, DragDropEffects.Move);
        }

        private void BoardColumn_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(Guid)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void BoardColumn_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Guid)))
            {
                return;
            }

            var flow = sender as FlowLayoutPanel;
            if (flow == null)
            {
                return;
            }

            var taskId = (Guid)e.Data.GetData(typeof(Guid));
            var task = _tasks.FirstOrDefault(item => item.Id == taskId);
            if (task == null)
            {
                return;
            }

            task.Status = (TaskStatus)flow.Tag;
            SaveAndRender();
        }

        private void NewTaskTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            AddTask();
        }

        private void NewTaskTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateClearButtonVisibility();
        }

        private void ApplyResponsiveLayout()
        {
            SuspendLayout();

            var mode = GetCurrentMode();
            var contentWidth = ClientSize.Width - 48;
            var sectionWidth = Math.Max(280, contentWidth - 24);
            var showMoreOffset = mode == ResponsiveMode.Phone ? 24 : mode == ResponsiveMode.Tablet ? 32 : 40;

            _contentPanel.Padding = new Padding(mode == ResponsiveMode.Phone ? 16 : 24, 24, mode == ResponsiveMode.Phone ? 16 : 24, 24);

            foreach (Control section in _contentPanel.Controls)
            {
                section.Width = sectionWidth;
                section.Margin = new Padding(0, 0, 0, 20);
            }

            ConfigureSummaryLayout(mode, sectionWidth);
            ConfigureAddLayout(sectionWidth);
            ConfigureTasksLayout(sectionWidth, showMoreOffset);
            ConfigureBoardLayout(mode, sectionWidth);
            UpdateClearButtonVisibility();

            ResumeLayout();
        }

        private void ConfigureSummaryLayout(ResponsiveMode mode, int sectionWidth)
        {
            _summaryCardsPanel.Width = sectionWidth;
            var cardWidth = mode == ResponsiveMode.Phone
                ? sectionWidth
                : mode == ResponsiveMode.Tablet
                    ? Math.Max(180, (sectionWidth - 16) / 2)
                    : Math.Max(200, (sectionWidth - 32) / 3);

            foreach (Control card in _summaryCardsPanel.Controls)
            {
                card.Width = cardWidth;
            }

            _summarySection.Height = 48 + _summaryCardsPanel.PreferredSize.Height + 8;
        }

        private void ConfigureAddLayout(int sectionWidth)
        {
            var inputCard = _addSection.Tag as Panel;
            if (inputCard == null)
            {
                return;
            }

            inputCard.Width = sectionWidth;
            _newTaskTextBox.Width = Math.Max(110, inputCard.Width - 140);
            _clearTaskButton.Left = inputCard.Width - _clearTaskButton.Width - 16;
            _addSection.Height = 48 + inputCard.Height;
        }

        private void ConfigureTasksLayout(int sectionWidth, int showMoreOffset)
        {
            _tasksListPanel.Width = sectionWidth;

            foreach (Control row in _tasksListPanel.Controls)
            {
                row.Width = sectionWidth;
            }

            _toggleTasksButton.Location = new Point(0, 48 + _tasksListPanel.PreferredSize.Height + showMoreOffset);
            _tasksSection.Height = _toggleTasksButton.Visible
                ? _toggleTasksButton.Bottom
                : 48 + _tasksListPanel.PreferredSize.Height;
        }

        private void ConfigureBoardLayout(ResponsiveMode mode, int sectionWidth)
        {
            _boardTable.SuspendLayout();
            _boardTable.Width = sectionWidth;

            if (mode == ResponsiveMode.Phone)
            {
                _boardTable.ColumnCount = 1;
                _boardTable.RowCount = 3;
                _boardTable.ColumnStyles.Clear();
                _boardTable.RowStyles.Clear();
                _boardTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                for (var i = 0; i < 3; i++)
                {
                    _boardTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 300F));
                }
            }
            else
            {
                _boardTable.ColumnCount = 3;
                _boardTable.RowCount = 1;
                _boardTable.ColumnStyles.Clear();
                _boardTable.RowStyles.Clear();
                for (var i = 0; i < 3; i++)
                {
                    _boardTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
                }
                _boardTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 340F));
            }

            for (var i = 0; i < _boardTable.Controls.Count; i++)
            {
                var control = _boardTable.Controls[i];
                if (mode == ResponsiveMode.Phone)
                {
                    _boardTable.SetColumn(control, 0);
                    _boardTable.SetRow(control, i);
                }
                else
                {
                    _boardTable.SetColumn(control, i);
                    _boardTable.SetRow(control, 0);
                }

                var panel = control as Panel;
                if (panel == null)
                {
                    continue;
                }

                panel.Width = mode == ResponsiveMode.Phone ? sectionWidth - 2 : (sectionWidth - 4) / 3;
                panel.Height = mode == ResponsiveMode.Phone ? 298 : 338;

                var flow = panel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
                if (flow != null)
                {
                    flow.Width = panel.Width - 32;
                    flow.Height = panel.Height - 68;
                }
            }

            _boardTable.Height = mode == ResponsiveMode.Phone ? 906 : 340;
            _boardSection.Height = 48 + _boardTable.Height;
            _boardTable.ResumeLayout();
        }

        private void UpdateClearButtonVisibility()
        {
            var mode = GetCurrentMode();
            var hasText = !string.IsNullOrWhiteSpace(_newTaskTextBox.Text);
            var shouldShow = hasText && (mode == ResponsiveMode.Phone || mode == ResponsiveMode.Tablet || _inputHovered || _newTaskTextBox.Focused);
            _clearTaskButton.Visible = shouldShow;
        }

        private static int GetStatusOrder(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Open:
                    return 0;
                case TaskStatus.InProgress:
                    return 1;
                case TaskStatus.Closed:
                    return 2;
                default:
                    return 3;
            }
        }

        private static Panel CreateSectionPanel()
        {
            return new Panel
            {
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
        }

        private static void AddSectionTitle(Control parent, string text)
        {
            var label = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 38, 59),
                Location = new Point(0, 0),
                Text = text
            };

            parent.Controls.Add(label);
        }

        private static Color GetStatusColor(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Open:
                    return Color.FromArgb(56, 132, 255);
                case TaskStatus.InProgress:
                    return Color.FromArgb(245, 158, 11);
                case TaskStatus.Closed:
                    return Color.FromArgb(34, 197, 94);
                default:
                    return Color.FromArgb(111, 124, 140);
            }
        }

        private static Color GetStatusLightColor(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Open:
                    return Color.FromArgb(232, 241, 255);
                case TaskStatus.InProgress:
                    return Color.FromArgb(255, 244, 222);
                case TaskStatus.Closed:
                    return Color.FromArgb(228, 247, 235);
                default:
                    return Color.FromArgb(243, 246, 251);
            }
        }

        private ResponsiveMode GetCurrentMode()
        {
            if (ClientSize.Width >= 1221)
            {
                return ResponsiveMode.Desktop;
            }

            if (ClientSize.Width >= 993)
            {
                return ResponsiveMode.Laptop;
            }

            if (ClientSize.Width >= 641)
            {
                return ResponsiveMode.Tablet;
            }

            return ResponsiveMode.Phone;
        }

        private enum ResponsiveMode
        {
            Desktop,
            Laptop,
            Tablet,
            Phone
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
