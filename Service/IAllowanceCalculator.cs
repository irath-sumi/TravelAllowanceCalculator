using Microsoft.AspNetCore.Mvc.Rendering;
using TravelAllowanceCalculator.Models;

namespace TravelAllowanceCalculator.Service
{
    public interface IAllowanceCalculator
    {
        DateTime GetFirstMondayOfNextMonth(int year, int month);
        decimal CalculateCompensation(decimal distance, string? transport,int distanceOneWay, IEnumerable<CompensationRate> rates);
        List<Employee>? GetEmployeeTravelData();
        Task<List<Result>> CalculateTravelAllowance(MonthModel monthyear);
        MemoryStream GenerateCSV(List<Result> data);
        List<SelectListItem> GetMonthList();
    }
}
