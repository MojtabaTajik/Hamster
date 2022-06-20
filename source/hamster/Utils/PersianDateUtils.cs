using System.Globalization;

namespace hamster.Utils;

public class PersianDateUtils
{
    public static string Now()
    {
        DateTime now = DateTime.Now;
        PersianCalendar pc = new PersianCalendar();
        
        return $"{pc.GetYear(now)}.{pc.GetMonth(now)}.{pc.GetDayOfMonth(now)}" +
               $"-{pc.GetHour(now)}.{pc.GetMinute(now)}.{pc.GetSecond(now)}";
    }
}