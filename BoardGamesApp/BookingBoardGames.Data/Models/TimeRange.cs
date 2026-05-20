using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
[Owned]
public class TimeRange
{
    public TimeRange() { }

    public TimeRange(DateTime startTime, DateTime endTime)
    {
        if (endTime < startTime) throw new ArgumentException("EndTime must be after StartTime");
        StartTime = startTime;
        EndTime = endTime;
    }

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("end_time")]
    public DateTime EndTime { get; set; }
}
