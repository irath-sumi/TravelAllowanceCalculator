namespace TravelAllowanceCalculator.Models
{
    public class CompensationRate
    {
        public int id { get; set; }
        // name here is the transport mode which is train, bus, bike or car
        public string? name { get; set; }
        public decimal base_compensation_per_km { get; set; }

    }
    public class Employee
    {
        public string? Name { get; set; }
        public string? Transport { get; set; }
        public int Distance { get; set; }
        public decimal Workdaysperweek { get; set; }
    }
    public class Result
    {
        public string? Employee { get; set; }
        public string? Transport { get; set; }
        public decimal Compensation { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal DistanceOneWay { get; set; }
        public decimal ToTalDistance { get; set; }
    }

}
