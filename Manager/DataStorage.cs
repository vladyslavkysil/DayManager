using  System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using DayManager.Models;

namespace DayManager.Managers
{
    public class DataStorage
    {
        private readonly string _filePath = "events.json";

        
        public  void SaveEvents(List<DiaryEvent> events)
        {
          var  options = new  JsonSerializerOptions { WriteIndented = true};
          string  jsonString  = JsonSerializer.Serialize(events, options);

          File.WriteAllText(_filePath, jsonString);  
        }
        
        public List<DiaryEvent> LoadEvents()
        {
            if(!File.Exists(_filePath))
            {
                return new  List<DiaryEvent>();
            }
            try
            {
                string jsonString = File.ReadAllText(_filePath);

                var  events = JsonSerializer.Deserialize<List<DiaryEvent>>(jsonString);

                return events ?? new List<DiaryEvent>();
            }

            catch (Exception)
            {
                return new  List<DiaryEvent>();
            }
        }
    }
}