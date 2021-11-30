using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel;

namespace LSSD.Lunch.Extensions
{
    public static class DateTimeExtensions
    {    
        public static DateTime AdjustForTimezone(this DateTime thisDate, string TimeZoneString) 
        {
            if (!string.IsNullOrEmpty(TimeZoneString))
            {
                try {
                    TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneString);
                    return AdjustForTimezone(thisDate, timeZone);
                }
                catch {}
            }

            return thisDate;
        } 

        public static DateTime AdjustForTimezone(this DateTime thisDate, TimeZoneInfo TimeZone) 
        {
            if (TimeZone != null) 
            {
                return TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(thisDate), TimeZone);
            }

            return thisDate;
        }        
    }
}
