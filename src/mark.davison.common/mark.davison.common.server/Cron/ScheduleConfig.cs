﻿namespace mark.davison.common.server.Cron;

public class ScheduleConfig<T> : IScheduleConfig<T>
{
    public string CronExpression { get; set; } = string.Empty;
    public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;
}