using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Globalization;
using TravelAllowanceCalculator.Models;

namespace TravelAllowanceCalculator.Service
{
    public class AllowanceCalculator : IAllowanceCalculator
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public AllowanceCalculator(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();

            // Read from appsettings the API Route
            _httpClient.BaseAddress = new Uri(_configuration.GetSection("Settings:ApiGETRoute").Value);

        }
        /// <summary>
        /// Calculate travel allowance for the employees for current month.
        /// Payment date is calulate as first Monday of subsequent month.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Result>> CalculateTravelAllowance(MonthModel model)
        {
            List<Result> results = new List<Result>();
            int weeksInYear = 52;

            // Get the compensation rates for different travel types from the API
            var response = await _httpClient.GetAsync("applicant/travel_types");
            var content = await response.Content.ReadAsStringAsync();
            IEnumerable<CompensationRate>? employeetravelModes = JsonConvert.DeserializeObject<IEnumerable<CompensationRate>>(content);

            // Get the first Monday of the next month for Payment Date calculation
            var paymentDate = GetFirstMondayOfNextMonth(model.Year, model.Month);

            // Get employee travel data
            List<Employee>? employeeTravelObjects = GetEmployeeTravelData();

            // Calculate the overall allowance information for each employee for a month
            if (employeeTravelObjects != null && employeetravelModes != null)
            {
                results = employeeTravelObjects.Select(employee =>
                {
                    var commuteDistanceInTheMonth = (employee.Distance * 2) * employee.Workdaysperweek * weeksInYear / 12;

                    var compensationPerMonth = CalculateCompensation(commuteDistanceInTheMonth,
                        employee.Transport, employee.Distance, employeetravelModes);

                    return new Result
                    {
                        Employee = employee.Name,
                        Transport = employee.Transport,
                        DistanceOneWay = employee.Distance,
                        ToTalDistance = commuteDistanceInTheMonth,
                        Compensation = compensationPerMonth,
                        PaymentDate = paymentDate
                    };
                }).ToList();
            }
            return results;

        }
        /// <summary>
        /// Generate allowance report
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public MemoryStream GenerateCSV(List<Result> results)
        {
            var csv = "Employee,Transport,(Avg)Travelled Distance Per Month,Compensation Per Month,Payment Date\n";
            foreach (var comp in results)
            {
                csv += $"{comp.Employee},{comp.Transport},{comp.ToTalDistance},{comp.Compensation},{comp.PaymentDate:yyyy-MM-dd}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            var stream = new MemoryStream(bytes);
            return stream;

        }


        /// <summary>
        /// The method fetches the date of first Monday of the subsequent month. 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public DateTime GetFirstMondayOfNextMonth(int year, int month)
        {
            var firstDayOfNextMonth = new DateTime(year, month, 1).AddMonths(1);
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)firstDayOfNextMonth.DayOfWeek + 7) % 7;
            var paymentDate = firstDayOfNextMonth.AddDays(daysUntilMonday);

            return paymentDate;
        }
        /// <summary>
        /// Travel Allowance is calculated here based on means of transport.
        /// </summary>
        /// <param name="totalDistanceTravelledPerMonth"></param>
        /// <param name="transport"></param>
        /// <param name="travelTypes"></param>
        /// <returns></returns>

        public decimal CalculateCompensation(decimal totalDistanceTravelledPerMonth, string? transport, int distanceOneWay, IEnumerable<CompensationRate>? rates)
        {
            decimal rate; decimal travelallowance;
            rate = (from b in rates
                    where b.name == transport?.ToLower()
                    select b.base_compensation_per_km).Single();
            if (transport?.ToLower() == "bike")
            {
                if (distanceOneWay > 0 && distanceOneWay <= 5)
                {
                    travelallowance = totalDistanceTravelledPerMonth * rate;
                }
                else
                {
                    // For distances between 5 to 10 km the compensation doubles
                    travelallowance = totalDistanceTravelledPerMonth * rate * 2;
                }

            }
            else
            {
                return totalDistanceTravelledPerMonth * rate;
            }

            return travelallowance;

        }
        /// <summary>
        /// Returns Employees weeksdays and commuting means data
        /// </summary>
        /// <returns></returns>

        public List<Employee>? GetEmployeeTravelData()
        {
            // Read json data file path from appsettings file
            string path = _configuration.GetSection("Settings:Datapath").Value;

            var dirName = Path.GetFullPath(path);
            var filePath = dirName.Replace("~", "");
            // Read employee travel data from .json file
            using (StreamReader reader = new StreamReader(filePath))
            {
                var json = reader.ReadToEnd();
                // Convert JSON string to list of objects
                List<Employee>? employeeTravelObjects = employeeTravelObjects = JsonConvert.DeserializeObject<List<Employee>>(json);

                return employeeTravelObjects;
            }

        }
        /// <summary>
        /// Prepares a list of all months in a year
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetMonthList()
        {
            var months = Enumerable.Range(1, 12)
                        .Select(i => new SelectListItem
                        {
                            Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i),
                            Value = i.ToString()
                        }).ToList();

            return months;
        }

    }


}
