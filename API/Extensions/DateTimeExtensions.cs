namespace API.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateOnly dateOfBirth)
    {
        var now = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
        var dob = int.Parse(dateOfBirth.ToString("yyyyMMdd"));
        return (now - dob) / 10000;
    }
}