using System;

namespace CentralBankPublicWebService.DTOs
{
    public class CreditHistoryResponse
    {
        public string DebtorJuridicTaxpayerIdentificationNumber { get; set; }
        public string DebtConcept { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
    }
}