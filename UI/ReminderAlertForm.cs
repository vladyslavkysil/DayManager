using System;
using System.Drawing;
using System.Windows.Forms;
using DayManager.Models;

namespace DayManager.UI
{
    public partial class RemindrAlertForm : Form
    {
        public RemindrAlertForm(DiaryEvent ev)
        {
            SetupUI(ev);
            System.Media.SystemSounds.Exclamation.Play();
        }

        private void SetupUI(DiaryEvent ev)
        {
            this.Text = "Нагадування!";
            this.Size = new Size(380, 180); 

            this.StartPosition = FormStartPosition.Manual; 
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;

            // Іконка (залишаємо зліва)
            Label lblIcon = new Label
            {
                Text = "⏰",
                Font = new Font("Segoe UI", 32), 
                Location = new Point(15, 20),
                AutoSize = true
            };
            this.Controls.Add(lblIcon);

            // Назва справи
            Label lblTitle = new Label
            {
                Text = ev.Title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(95, 25), 
                AutoSize = true,
                MaximumSize = new Size(250, 0) 
            };
            this.Controls.Add(lblTitle);

            // Деталі 
            Label lblDetails = new Label
            {
                Text = $"Час: {ev.StartTime.ToString(@"hh\:mm")}\nМісце: {(string.IsNullOrWhiteSpace(ev.Location) ? "Не вказано" : ev.Location)}",
                Location = new Point(95, 55),
                AutoSize = true
            };
            this.Controls.Add(lblDetails);

            // Кнопка 
            Button btnOk = new Button
            {
                Text = "Зрозуміло",
                Location = new Point(135, 100),
                Width = 110,
                Height = 35,
                BackColor = Color.LightSkyBlue,
                Cursor = Cursors.Hand
            };
            btnOk.Click += (s, e) => this.Close();
            this.Controls.Add(btnOk);

            // вікно в кут з невеликим відступом
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(
                workingArea.Right - this.Size.Width - 15,
                workingArea.Bottom - this.Size.Height - 15
            );
        }
    }
}