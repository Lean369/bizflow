using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    /// <summary>
    /// TaskScheduler is a singleton that keeps references on the timers so timers will not be garbage collected, it can schedule multiple tasks.
    /// You can set the first run (hour and minute), if at the time of scheduling this time is over scheduling start on the next day this at that time.
    /// </summary>
    public class TaskScheduler
    {
        private static TaskScheduler _instance;
        private List<System.Threading.Timer> timers = new List<System.Threading.Timer>();

        private TaskScheduler() { }

        public static TaskScheduler Instance => _instance ?? (_instance = new TaskScheduler());

        public void ScheduleTask(int hour, int min, double intervalInHour, Action task)
        {
            DateTime now = DateTime.Now;
            DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, 0);
            if (now > firstRun)
            {
                firstRun = firstRun.AddDays(1);
            }

            TimeSpan timeToGo = firstRun - now;
            if (timeToGo <= TimeSpan.Zero)
            {
                timeToGo = TimeSpan.Zero;
            }

            var timer = new System.Threading.Timer(x =>
            {
                task.Invoke();
            }, null, timeToGo, TimeSpan.FromHours(intervalInHour));

            timers.Add(timer);
        }
    }
}
