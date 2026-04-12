using  System;
using  System.Drawing;
using System.Net.Http.Headers;
using System.Windows.Forms;
using DayManager.Models;

namespace DayManager.UI
{
    public partial class RemindrAlertForm : Form
    {

        public RemindrAlertForm(DiaryEvent ev){
        SetupUI(ev);

        //Звук при появі
        System.Media.SystemSounds.Exclamation.Play();
    }
    

    private void SetupUI(DiaryEvent ev)
        {
            this.Text = "";
            this.Size = new Size(350, 150);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            //завжди зверху

            this.TopMost = true;

            Label lblIcon = new Label
            {
                Text = "ПОРА",
                Font = new Font("Segoe UI", 24),
                Location = new Point(10, 15),
                AutoSize = true
            };
            this.Controls.Add(lblIcon);

            //Назва справи
            Label lblTitle = new Label
            {
                Text = ev.Title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(60,20),
                AutoSize = true,
                MaximumSize = new Size(260,0)

            };
            this.Controls.Add(lblTitle);

            Label lblDetails = new Label
            {
                Text = $"Час: {ev.StartTime.ToString(@"hh\:m")}\nМісце:{ev.Location}",
                Location = new Point(60,50),
                AutoSize = true
            };
            this.Controls.Add(lblDetails);
        

        Button btnOk = new Button
        {
            Text = "Зрозуміло",
            Location = new Point(120,80),
            Width = 100,
            BackColor = Color.LightSkyBlue
        };
        btnOk.Click += (s, e) => this.Close();
        this.Controls.Add(btnOk);
    }
  }
}