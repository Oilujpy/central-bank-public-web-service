using System;

namespace CentralBankPublicWebService.DTOs
{
    public class FinancialHealthResult
    {
        public string Indicator { get; set; }
        public string Comment { get; set; }
        public decimal TotalAmount { get; set; }
    }
}