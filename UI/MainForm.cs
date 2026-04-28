using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using DayManager.Managers;
using DayManager.Models;

namespace DayManager.UI
{
    public partial class MainForm : Form
    {
        private MonthCalendar _calendar;
        private DataGridView _scheduleGrid;
        private Button _btnAddEvent;
        private Button _btnToggleView;
        private ContextMenuStrip _gridContextMenu;
        private GroupBox _gbOverdue;
        private ListBox _lbOverdue;

        private bool _isWeeklyView = false;

        private EventManager _eventManager;
        private ReminderTimer _reminderTimer;

        public MainForm()
        {
            _eventManager = new EventManager();

            _reminderTimer = new ReminderTimer(_eventManager);
            _reminderTimer.OnEventReminder += ShowReminder;
            
            SetupUI();
            
            UpdateGridColumns(); 
            UpdateEventsList(_calendar.SelectionStart);
        }

        private void SetupUI()
        {
            this.Text = "Щоденник (DayManager)";
            this.Size = new Size(850, 550); 
            this.StartPosition = FormStartPosition.CenterScreen;

            _calendar = new MonthCalendar
            {
                Location = new Point(20, 20),
                MaxSelectionCount = 1
            };
            _calendar.DateChanged += Calendar_DateChanged;

            _btnToggleView = new Button
            {
                Text = "Показати тиждень",
                Location = new Point(20, 200),
                Size = new Size(164, 40),
                BackColor = Color.LightSkyBlue,
                Cursor = Cursors.Hand
            };
            _btnToggleView.Click += BtnToggleView_Click;

            _btnAddEvent = new Button
            {
                Text = "Додати нову подію",
                Location = new Point(250, 20),
                Size = new Size(150, 40),
                BackColor = Color.LightGreen,
                Cursor = Cursors.Hand
            };
            _btnAddEvent.Click += BtnAddEvent_Click; 

            _scheduleGrid = new DataGridView
            {
                Location = new Point(250, 70),
                Size = new Size(550, 400),
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True },
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            };

            _gridContextMenu = new ContextMenuStrip();
            _scheduleGrid.ContextMenuStrip = _gridContextMenu;

            _scheduleGrid.CellMouseDown += (s, e) =>
            {
                if(e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    _scheduleGrid.CurrentCell = _scheduleGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    _gridContextMenu.Items.Clear();

                    if (_scheduleGrid.CurrentCell.Tag is System.Collections.Generic.List<DiaryEvent> eventsInCell)
                    {
                        if (eventsInCell.Count == 1)
                        {
                            var ev = eventsInCell[0];
                            _gridContextMenu.Items.Add("Редагувати", null, (sender, args) => EditEventObject(ev));
                            _gridContextMenu.Items.Add("Видалити", null, (sender, args) => DeleteEventObject(ev));
                        }
                        else
                        {
                            foreach (DiaryEvent ev in eventsInCell)
                            {
                                var item = new ToolStripMenuItem($"[{ev.StartTime:hh\\:mm}] {ev.Title}");
                                item.DropDownItems.Add("Редагувати", null, (sender, args) => EditEventObject(ev));
                                item.DropDownItems.Add("Видалити", null, (sender, args) => DeleteEventObject(ev));
                                _gridContextMenu.Items.Add(item);
                            }
                        }
                    }
                }
            };

            _gbOverdue = new GroupBox
            {
                Text = "Прострочені справи",
                Location = new Point(20, 260),
                Size = new Size(200, 200),
                Visible = false
            };

            _lbOverdue = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };

            _lbOverdue.DoubleClick += LbOverdue_DoubleClick;

            _gbOverdue.Controls.Add(_lbOverdue);

            this.Controls.Add(_calendar);
            this.Controls.Add(_btnAddEvent);
            this.Controls.Add(_btnToggleView);
            this.Controls.Add(_scheduleGrid);
            this.Controls.Add(_gbOverdue);

            CheckPastTasks();
        }

        private void EditEventObject(DiaryEvent ev)
        {
            using (var dialog = new EventDialogForm(ev.Date))
            {
                dialog.LoadEventData(ev); 

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    dialog.ResultEvent.Id = ev.Id;

                    if (_eventManager.UpdateEvent(dialog.ResultEvent))
                    {
                        UpdateEventsList(_calendar.SelectionStart);
                        CheckPastTasks();
                    }
                    else
                    {
                        MessageBox.Show("Цей час уже зайнятий!", "Накладка");
                    }
                }
            }
        }

        private void DeleteEventObject(DiaryEvent ev)
        {
            var result = MessageBox.Show($"Видалити подію '{ev.Title}'?",
            "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _eventManager.DeleteEvent(ev.Id);
                UpdateEventsList(_calendar.SelectionStart);
                CheckPastTasks();
            }
        }

        private void LbOverdue_DoubleClick(object? sender, EventArgs e)
        {
            if (_lbOverdue.SelectedItem != null)
            {
                dynamic selectedItem = _lbOverdue.SelectedItem;
                DiaryEvent overdueEvent = selectedItem.Event;
                EditEventObject(overdueEvent);
            }
        }

        private void CheckPastTasks()
        {
            var pastEvents = _eventManager.GetAllEvents()
            .Where(e => e.Date.Date < DateTime.Today.Date)
            .OrderBy(e => e.Date) 
            .ToList();

            if (pastEvents.Count > 0 )
            {
                _gbOverdue.Visible = true;
                _lbOverdue.Items.Clear();
                foreach(var ev in pastEvents)
                {
                    _lbOverdue.Items.Add(new {Event = ev, Display = $"{ev.Date:dd.MM} - {ev.Title}"});
                }
                _lbOverdue.DisplayMember = "Display";
            }
            else
            {
                _gbOverdue.Visible = false;
            }
        }

        private void BtnToggleView_Click(object? sender, EventArgs e)
        {
            _isWeeklyView = !_isWeeklyView;
            
            _btnToggleView.Text = _isWeeklyView ? "Показати день" : "Показати тиждень";

            UpdateGridColumns();
            UpdateEventsList(_calendar.SelectionStart);
        }

        private void UpdateGridColumns()
        {
            _scheduleGrid.Columns.Clear();
            _scheduleGrid.Rows.Clear();

            _scheduleGrid.Columns.Add("TimeCol", "Час");
            _scheduleGrid.Columns["TimeCol"].Width = 60;

            if (_isWeeklyView)
            {
                string[] days = {"Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд"};
                foreach (var day in days)
                {
                    _scheduleGrid.Columns.Add($"Col_{day}", day);
                }
            }
            else
            {
                _scheduleGrid.Columns.Add("DayCol", _calendar.SelectionStart.ToString("dd MMM yyyy"));
            }

            for (int i = 8; i <= 20; i++)
            {
                _scheduleGrid.Rows.Add($"{i}:00");
            }
        }

        private void Calendar_DateChanged(object? sender, DateRangeEventArgs e)
        {
            if (!_isWeeklyView)
            {
                UpdateGridColumns();
            }
            UpdateEventsList(e.Start);
        }

        private void UpdateEventsList(DateTime date)
        {
            foreach (DataGridViewRow row in _scheduleGrid.Rows)
            {
                for (int i = 1; i < _scheduleGrid.Columns.Count; i++)
                {
                    row.Cells[i].Value = "";
                    row.Cells[i].Tag = null;
                    row.Cells[i].Style.BackColor = Color.White;
                }
            }

            if (_isWeeklyView)
            {
                int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime startOfWeek = date.AddDays(-1 * diff).Date;

                for (int i = 0; i < 7; i++)
                {
                    FillGridWithEvents(startOfWeek.AddDays(i), i + 1); 
                }
            }
            else
            {
                FillGridWithEvents(date, 1);
            }
        }

        private void FillGridWithEvents(DateTime date, int columnIndex)
        {
            var dailyEvents = _eventManager.GetEventsByDate(date);
            foreach (var ev in dailyEvents)
            {
                int rowIndex = ev.StartTime.Hours - 8;

                if (rowIndex >= 0 && rowIndex < _scheduleGrid.Rows.Count)
                {
                   var cell = _scheduleGrid.Rows[rowIndex].Cells[columnIndex];
                  
                  if(cell.Tag == null)
                    {
                        cell.Tag = new System.Collections.Generic.List<DiaryEvent>();
                        cell.Value = "";
                    }

                     var eventsInCell = (System.Collections.Generic.List<DiaryEvent>)cell.Tag;
                     eventsInCell.Add(ev);

                     string timeStr = ev.StartTime.ToString(@"hh\:mm");
                     string locStr = string.IsNullOrWhiteSpace(ev.Location) ? "" :$" ({ev.Location})";
                     string eventText = $"[{timeStr}] {ev.Title}{locStr}";

                    if (string.IsNullOrEmpty(cell.Value?.ToString()))
                    {
                        cell.Value = eventText;
                    }
                    else
                    {
                        cell.Value += $"\n{eventText}";
                    }
                    cell.Style.BackColor = Color.LightYellow;
                }
            }
        }

        private void BtnAddEvent_Click(object? sender, EventArgs e)
        {
            using (var dialog = new EventDialogForm(_calendar.SelectionStart))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    DiaryEvent newEvent = dialog.ResultEvent;

                    if (_eventManager.AddEvent(newEvent))
                    {
                        UpdateEventsList(_calendar.SelectionStart);
                        CheckPastTasks();
                    }
                    else
                    {
                        MessageBox.Show("Помилка! Цей час зайнятий.",
                        "Накладка часу",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void ShowReminder(DiaryEvent ev)
        {
            this.Invoke((MethodInvoker)delegate
            {
                var alert = new RemindrAlertForm(ev);
                alert.Show();
            });
        }
    }
}