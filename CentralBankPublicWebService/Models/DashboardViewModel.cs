using System.Collections.Generic;

namespace CentralBankPublicWebService.Models
{
    public class DashboardViewModel
    {
        public List<BarChartData> BarChartData { get; set; } = new List<BarChartData>();
    }

    public class BarChartData
    {
        public string Caption { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }
}