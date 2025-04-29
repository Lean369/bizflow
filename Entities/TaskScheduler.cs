using System;
using System.Collections.Generic;
using System.Threading;

namespace Entities
{
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


    public class CronScheduler
    {
        private readonly Action _task;
        private readonly Timer _timer;
        private readonly string _cronExpression;

        public CronScheduler(string cronExpression, Action task)
        {
            _task = task;
            _cronExpression = cronExpression;
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            SetTimer();
        }

        private void TimerCallback(object state)
        {
            _task.Invoke();
            SetTimer();
        }

        private void SetTimer()
        {
            var nextExecution = GetNextExecutionTime();
            var delay = nextExecution - DateTime.Now;
            _timer.Change((int)delay.TotalMilliseconds, Timeout.Infinite);
        }

        private DateTime GetNextExecutionTime()
        {
            var now = DateTime.Now;

            // Parse the cron expression
            var cronParts = _cronExpression.Split(' ');
            if (cronParts.Length != 5)
            {
                throw new ArgumentException("Invalid cron expression format.");
            }

            var minute = cronParts[0];
            var hour = cronParts[1];
            var dayOfMonth = cronParts[2];
            var month = cronParts[3];
            var dayOfWeek = cronParts[4];

            // Calculate the next execution time based on the current time
            var nextExecution = now.AddMinutes(1); // Start with next minute
            while (true)
            {
                if (IsMatch(minute, nextExecution.Minute) &&
                    IsMatch(hour, nextExecution.Hour) &&
                    IsMatch(dayOfMonth, nextExecution.Day) &&
                    IsMatch(month, nextExecution.Month) &&
                    IsMatch(dayOfWeek, (int)nextExecution.DayOfWeek))
                {
                    break; // Found the next execution time
                }
                nextExecution = nextExecution.AddMinutes(1); // Try the next minute
            }

            return nextExecution;
        }

        private bool IsMatch(string cronField, int value)
        {
            if (cronField == "*")
            {
                return true; // match all
            }

            var parts = cronField.Split(',');
            foreach (var part in parts)
            {
                if (part.Contains("-"))
                {
                    var range = part.Split('-');
                    var start = int.Parse(range[0]);
                    var end = int.Parse(range[1]);
                    if (value >= start && value <= end)
                    {
                        return true; // match range
                    }
                }
                else if (part.Contains("/"))
                {
                    var stepParts = part.Split('/');
                    var start = int.Parse(stepParts[0]); //podria ser un asterisco
                    var step = int.Parse(stepParts[1]);
                    if ((value - start) % step == 0)
                    {
                        return true; // match step
                    }
                }
                else if (int.Parse(part) == value)
                {
                    return true; // match exact value
                }
            }

            return false;
        }
    }


}
