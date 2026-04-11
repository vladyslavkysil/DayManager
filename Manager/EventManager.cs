using  System;
using  System.Collections.Generic;
using  System.Linq;
using DayManager.Managers;
using DayManager.Models;

namespace DayManager
{
    public  class EventManager
    {
        private List <DiaryEvent> _events;

        private readonly DataStorage _storage;

        public EventManager()
        {
            _storage = new DataStorage();
            _events = _storage.LoadEvents(); // загрузка  сохран задач
        }

        public  List<DiaryEvent> GetEventsByDate(DateTime date)
        {
            return _events.Where(e => e.Date.Date == date.Date)
                          .OrderBy(e => e.StartTime)
                          .ToList();  
        }

        public bool CheckOverlaps(DateTime date, TimeSpan startTime, int durationMinutes, string? excludeEventId = null)
        {
           //время  конца задачи
            TimeSpan endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));

            foreach(var ev  in GetEventsByDate(date))
            {
                if(ev.Id == excludeEventId) continue;

                if(startTime < ev.EndTime && endTime > ev.StartTime)
                {
                    return true;// нашли накладку  во времени
                }
            }
            return false;
        }

        public bool AddEvent(DiaryEvent newEvent)
        {
            if (CheckOverlaps(newEvent.Date, newEvent.StartTime, newEvent.DurationMinutes))
            {
                return false;
            }

            _events.Add(newEvent);
            _storage.SaveEvents(_events);
            return true;
        }

        // обновление старой  задачи
        public bool UpdateEvent(DiaryEvent updateEvent)
        {
            if (CheckOverlaps(updateEvent.Date, updateEvent.StartTime, updateEvent.DurationMinutes))
            {
                return false;
            }

            var index = _events.FindIndex(e => e.Id == updateEvent.Id);
            if (index != -1)
            {
                _events[index] = updateEvent;
                _storage.SaveEvents(_events);
                return true;
            }
            return false;
        }

        //удаление задачи

        public void DiaryEvent(string eventId)
        {
            var ev  = _events.FirstOrDefault(e => e.Id == eventId);
            
                if (ev != null)
                {
                    _events.Remove(ev);
                    _storage.SaveEvents(_events);
                }
        
        }
    }
}