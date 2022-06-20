using System.Globalization;

namespace hamster.Utils;

public class PersianDateUtils
{
    public static string PersianToday()
    {
        DateTime now = DateTime.Now;
        PersianCalendar pc = new PersianCalendar();
        
        return $"{pc.GetYear(now)}.{pc.GetMonth(now)}.{pc.GetDayOfMonth(now)}";
    }
}