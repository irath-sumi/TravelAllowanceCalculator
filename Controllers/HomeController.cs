using Microsoft.AspNetCore.Mvc;
using TravelAllowanceCalculator.Models;
using TravelAllowanceCalculator.Service;

namespace TravelAllowanceCalculator.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAllowanceCalculator _allowanceCalculator;
        public HomeController(IAllowanceCalculator allowanceCalculator)
        {
            _allowanceCalculator = allowanceCalculator;
        }

        public IActionResult Index()
        {
            // service method call to fetch the employeed data, seeded from .json file
            var employeeTravelObjects = _allowanceCalculator.GetEmployeeTravelData();
            ViewBag.EmployeeTravelData = employeeTravelObjects;

            // Set the default month to the current month
            var defaultMonth = new MonthModel
            {
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year
            };

            ViewBag.Months = _allowanceCalculator.GetMonthList();
            ViewBag.SelectedMonth = defaultMonth;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Download(MonthModel model)
        {
            var data = await _allowanceCalculator.CalculateTravelAllowance(model);
            var stream = _allowanceCalculator.GenerateCSV(data);
            return File(stream, "text/csv", "travel_allowance_" + model.Month + "_" + model.Year + ".csv");
        }            

    }

}