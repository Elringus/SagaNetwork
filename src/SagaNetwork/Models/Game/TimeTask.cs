using System;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Represents a process completed over time.
    /// </summary>
    [GenerateApi(UClassType.Struct)]
    public class TimeTask
    {
        public TimeSpan TaskDuration { get; set; }
        public DateTime LastCheckDate { get; set; }
        public TimeSpan RemainingTime { get; set; }

        public TimeTask () { }
        public TimeTask (TimeSpan taskDuration)
        {
            TaskDuration = taskDuration;
            LastCheckDate = DateTime.UtcNow;
            RemainingTime = TaskDuration;
        }

        /// <summary>
        /// Updates state timers and counts how much cycles passed since last check.
        /// </summary>
        /// <returns>Number of cycles passed since last check.</returns>
        public virtual int Check ()
        {
            var cyclesPassed = 0;
            var timeSinceLastCheck = DateTime.UtcNow - LastCheckDate;

            if (RemainingTime <= timeSinceLastCheck)
            {
                cyclesPassed = 1;
                timeSinceLastCheck -= RemainingTime;
                while (timeSinceLastCheck >= TaskDuration)
                {
                    timeSinceLastCheck -= TaskDuration;
                    cyclesPassed++;
                }
            }

            LastCheckDate = DateTime.UtcNow;
            RemainingTime = TaskDuration - timeSinceLastCheck;

            return cyclesPassed;
        }
    }
}
