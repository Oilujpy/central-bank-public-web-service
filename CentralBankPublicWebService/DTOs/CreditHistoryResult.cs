using System;

namespace CentralBankPublicWebService.DTOs
{
    public class CreditHistoryResult
    {
        public string DebtorJuridicTaxpayerIdentificationNumber { get; set; }
        public string DebtConcept { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
    }
}