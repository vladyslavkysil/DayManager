using System;
using System.Drawing;
using System.Windows.Forms;
using DayManager.Managers;
using DayManager.Models;

namespace DayManager.UI
{
    public partial class MainForm : Form
    {
        // Візуальні елементи
        private MonthCalendar _calendar;
        private DataGridView _scheduleGrid;
        private Button _btnAddEvent;
        private Button _btnToggleView;

        // Стан перегляду
        private bool _isWeeklyView = false;

        // Ядро програми
        private EventManager _eventManager;
        private ReminderTimer _reminderTimer;

        public MainForm()
        {
            _eventManager = new EventManager();

            // Ініціалізація таймера нагадувань
            _reminderTimer = new ReminderTimer(_eventManager);
            _reminderTimer.OnEventReminder += ShowReminder;
            
            SetupUI();
            
            // Налаштовуємо колонки та завантажуємо події для обраної дати
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

            this.Controls.Add(_calendar);
            this.Controls.Add(_btnAddEvent);
            this.Controls.Add(_btnToggleView);
            this.Controls.Add(_scheduleGrid);
        }

        private void BtnToggleView_Click(object? sender, EventArgs e)
        {
            _isWeeklyView = !_isWeeklyView;
            
            // Зміна тексту на кнопці
            _btnToggleView.Text = _isWeeklyView ? "Показати день" : "Показати тиждень";

            UpdateGridColumns();
            UpdateEventsList(_calendar.SelectionStart);
        }

        private void UpdateGridColumns()
        {
            _scheduleGrid.Columns.Clear();
            _scheduleGrid.Rows.Clear();

            // Перша колонка (час)
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
                // Режим дня 
                _scheduleGrid.Columns.Add("DayCol", _calendar.SelectionStart.ToString("dd MMM yyyy"));
            }

            // Додаємо рядки для годин (з 8:00 до 20:00)
            for (int i = 8; i <= 20; i++)
            {
                _scheduleGrid.Rows.Add($"{i}:00");
            }
        }

        private void Calendar_DateChanged(object? sender, DateRangeEventArgs e)
        {
            // Якщо ми в режимі дня, треба оновити заголовок колонки
            if (!_isWeeklyView)
            {
                UpdateGridColumns();
            }
            UpdateEventsList(e.Start);
        }

        private void UpdateEventsList(DateTime date)
        {
            // Очищаємо комірки від старих даних
            foreach (DataGridViewRow row in _scheduleGrid.Rows)
            {
                for (int i = 1; i < _scheduleGrid.Columns.Count; i++)
                {
                    row.Cells[i].Value = "";
                    row.Cells[i].Style.BackColor = Color.White;
                }
            }

            if (_isWeeklyView)
            {
                // Знаходимо понеділок обраного тижня
                int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime startOfWeek = date.AddDays(-1 * diff).Date;

                // Заповнюємо дані для кожного дня тижня
                for (int i = 0; i < 7; i++)
                {
                    DateTime currentDay = startOfWeek.AddDays(i);
                    FillGridWithEvents(currentDay, i + 1); // Колонки йдуть з 1 до 7
                }
            }
            else
            {
                // Режим дня (колонка 1)
                FillGridWithEvents(date, 1);
            }
        }

        // Допоміжний метод для розміщення подій у конкретну колонку таблиці
        private void FillGridWithEvents(DateTime date, int columnIndex)
        {
            var dailyEvents = _eventManager.GetEventsByDate(date);
            foreach (var ev in dailyEvents)
            {
                // Визначаємо рядок на основі часу (рядок 0 - це 8:00)
                int rowIndex = ev.StartTime.Hours - 8;

                // Перевіряємо, чи подія входить у наш робочий графік (8:00 - 20:00)
                if (rowIndex >= 0 && rowIndex < _scheduleGrid.Rows.Count)
                {
                    string locationText = string.IsNullOrWhiteSpace(ev.Location) ? "" : $"\n({ev.Location})";
                    _scheduleGrid.Rows[rowIndex].Cells[columnIndex].Value = $"{ev.Title}{locationText}";
                    _scheduleGrid.Rows[rowIndex].Cells[columnIndex].Style.BackColor = Color.LightYellow;
                }
            }
        }

        private void BtnAddEvent_Click(object? sender, EventArgs e)
        {
            // Спливаюче вікно для додавання
            using (var dialog = new EventDialogForm(_calendar.SelectionStart))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    DiaryEvent newEvent = dialog.ResultEvent;

                    if (_eventManager.AddEvent(newEvent))
                    {
                        UpdateEventsList(_calendar.SelectionStart);
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
            // Відмальовка в основному потоці
            this.Invoke((MethodInvoker)delegate
            {
                var alert = new RemindrAlertForm(ev);
                alert.Show();
            });
        }
    }
}