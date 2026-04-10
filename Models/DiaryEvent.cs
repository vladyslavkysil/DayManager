using  System;

namespace DayManager.Models
{
    public  class  DiaryEvent
    {
        public  string Id  {get; set;} = Guid.NewGuid().ToString(); // уник  ид
        public  string  Title  {get; set;} = string.Empty; // имя
        public DateTime Date {get; set;} // дата  
        public TimeSpan StartTime {get ; set;} // время  начала

        public  int DurationMinutes {get; set;} // времи  в  мин
        public  string  Location {get; set;} = string.Empty; //место
        public  TimeSpan EndTime => StartTime.Add(TimeSpan.FromMinutes(DurationMinutes));


    }
}