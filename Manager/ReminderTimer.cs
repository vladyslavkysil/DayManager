using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DayManager.Models;


namespace DayManager.Managers
{
    public class ReminderTimer
    {
        private System.Windows.Forms.Timer _timer;
        private EventManager _eventManager;

        //для  тих про кого вже нагадали
        private HashSet<string> _notifiedEventIds;

        public  Action<DiaryEvent>? OnEventReminder;

        public ReminderTimer(EventManager eventManager)
        {
            _eventManager = eventManager;
            _notifiedEventIds = new HashSet<string>();

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 10000;
            _timer.Tick += CheckUpcomingEvents;
            _timer.Start();


        }

        private void CheckUpcomingEvents(object? sender, EventArgs e)
        {
            var  todayEvents = _eventManager.GetEventsByDate(DateTime.Today);
            TimeSpan currentTime = DateTime.Now.TimeOfDay;


            foreach (var ev in todayEvents)
            {
                TimeSpan timeDifference = ev.StartTime - currentTime;

                if(timeDifference.TotalMinutes <= 15 && timeDifference.TotalMinutes >= -5)
                {
                    if (!_notifiedEventIds.Contains(ev.Id))
                    {
                        _notifiedEventIds.Add(ev.Id);

                        OnEventReminder?.Invoke(ev);
                    }
                }
                
            }

            
        }
    }
}