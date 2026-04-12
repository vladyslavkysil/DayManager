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
        private ListBox _eventsListBox;
        private Button _btnAddEvent;

        // Ядро програми
        private EventManager _eventManager;

        public MainForm()
        {
            _eventManager = new EventManager();
            SetupUI();
            
            // Завантажуємо події для обраної дати під час старту
            UpdateEventsList(_calendar.SelectionStart);
        }

        private void SetupUI()
        {
            this.Text = "Щоденник (DayManager)";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            _calendar = new MonthCalendar
            {
                Location = new Point(20, 20),
                MaxSelectionCount = 1
            };
            _calendar.DateChanged += Calendar_DateChanged;

            _btnAddEvent = new Button
            {
                Text = "Додати нову подію",
                Location = new Point(250, 20),
                Size = new Size(150, 40),
                BackColor = Color.LightGreen,
                Cursor = Cursors.Hand
            };
              _btnAddEvent.Click += BtnAddEvent_Click; 

            _eventsListBox = new ListBox
            {
                Location = new Point(250, 70),
                Size = new Size(500, 350),
                Font = new Font("Segoe UI", 12)
            };

            this.Controls.Add(_calendar);
            this.Controls.Add(_btnAddEvent);
            this.Controls.Add(_eventsListBox);
        }

        private void Calendar_DateChanged(object? sender, DateRangeEventArgs e)
        {
            UpdateEventsList(e.Start);
        }

        private void UpdateEventsList(DateTime date)
        {
            _eventsListBox.Items.Clear();

            var dailyEvents = _eventManager.GetEventsByDate(date);
            if (dailyEvents.Count == 0)
            {
                _eventsListBox.Items.Add("Сьогодні справ немає!");
            }
            else
            {
                foreach (var ev in dailyEvents)
                {
                    string timeString = ev.StartTime.ToString(@"hh\:mm");
                    string itemText = $"{timeString} | {ev.Title} (Місце: {ev.Location})";
                    _eventsListBox.Items.Add(itemText);
                }
            }
        }
        private void  BtnAddEvent_Click(object? sender, EventArgs e)
        {
            //спливаюче вікно
            using (var dialog  = new EventDialogForm(_calendar.SelectionStart))
            {
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    //якщо користувач  зберіг
                 DiaryEvent newEvent = dialog.ResultEvent;

                    if (_eventManager.AddEvent(newEvent))
                    {
                        UpdateEventsList(_calendar.SelectionStart);
                    }
                    else
                    {
                        MessageBox.Show("Помилка! Це час зайнятий.",
                        "Накладка часу",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    }

                }
            }
        }

    }
}
