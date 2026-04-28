using System;
using System.Drawing;
using System.Windows.Forms;
using DayManager.Models;

namespace DayManager.UI
{
    public partial class EventDialogForm : Form
    {
        // Поля для вводу даних
        private TextBox _txtTitle;
        private DateTimePicker _dtpDate;
        private DateTimePicker _dtpStartTime;
        private NumericUpDown _numDuration;
        private TextBox _txtLocation;
        
        // Кнопки
        private Button _btnSave;
        private Button _btnCancel;

        public DiaryEvent ResultEvent { get; private set; }

        public EventDialogForm(DateTime selectedDate)
        {
            SetupUI();
            
            _dtpDate.Value = selectedDate;
        }

        private void SetupUI()
        {
            this.Text = "Додати/Редагувати подію";
            this.Size = new Size(350, 400);
            this.StartPosition = FormStartPosition.CenterParent; // З'явиться по центру головного вікна
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Робимо вікно незмінним у розмірах
            this.MaximizeBox = false;

            int startY = 20;

            // 1. Назва
            this.Controls.Add(new Label { Text = "Назва події:", Location = new Point(20, startY), AutoSize = true });
            _txtTitle = new TextBox { Location = new Point(20, startY + 20), Width = 290 };
            this.Controls.Add(_txtTitle);

            // 2. Дата
            this.Controls.Add(new Label { Text = "Дата:", Location = new Point(20, startY + 60), AutoSize = true });
            _dtpDate = new DateTimePicker { Location = new Point(20, startY + 80), Width = 140, Format = DateTimePickerFormat.Short };
            this.Controls.Add(_dtpDate);

            // 3. Час початку
            this.Controls.Add(new Label { Text = "Час початку:", Location = new Point(170, startY + 60), AutoSize = true });
            _dtpStartTime = new DateTimePicker 
            { 
                Location = new Point(170, startY + 80), 
                Width = 140, 
                Format = DateTimePickerFormat.Custom, 
                CustomFormat = "HH:mm", 
                ShowUpDown = true // Стрілочки замість календаря для вибору часу
            };
            this.Controls.Add(_dtpStartTime);

            // 4. Тривалість (у хвилинах)
            this.Controls.Add(new Label { Text = "Тривалість (хвилин):", Location = new Point(20, startY + 120), AutoSize = true });
            _numDuration = new NumericUpDown { Location = new Point(20, startY + 140), Width = 140, Minimum = 1, Maximum = 1440, Value = 60 };
            this.Controls.Add(_numDuration);

            // 5. Місце проведення
            this.Controls.Add(new Label { Text = "Місце проведення:", Location = new Point(20, startY + 180), AutoSize = true });
            _txtLocation = new TextBox { Location = new Point(20, startY + 200), Width = 290 };
            this.Controls.Add(_txtLocation);

            // 6. Кнопка Зберегти
            _btnSave = new Button { Text = "Зберегти", Location = new Point(20, startY + 260), Width = 140, Height = 40, BackColor = Color.LightGreen };
            _btnSave.Click += BtnSave_Click;
            this.Controls.Add(_btnSave);

            // 7. Кнопка Скасувати
            _btnCancel = new Button { Text = "Скасувати", Location = new Point(170, startY + 260), Width = 140, Height = 40 };
            _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(_btnCancel);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Базова перевірка, щоб користувач не створив порожню подію
            if (string.IsNullOrWhiteSpace(_txtTitle.Text))
            {
                MessageBox.Show("Будь ласка, введіть назву події!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Формування об'єкт події з даних, які ввів користувач
            ResultEvent = new DiaryEvent
            {
                Title = _txtTitle.Text.Trim(),
                Date = _dtpDate.Value.Date,
                StartTime = _dtpStartTime.Value.TimeOfDay,
                DurationMinutes = (int)_numDuration.Value,
                Location = _txtLocation.Text.Trim()
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public void LoadEventData(DiaryEvent ev)
        {
            _txtTitle.Text = ev.Title;
            _dtpDate.Value = ev.Date;

            //Час  початку

            _dtpStartTime.Value = DateTime.Today.Add(ev.StartTime);
            _numDuration.Value = ev.DurationMinutes;
            _txtLocation.Text = ev.Location;

            this.Text = "Редагувати  подію";
            _btnSave.Text = "Оновити";
        }
    }
}