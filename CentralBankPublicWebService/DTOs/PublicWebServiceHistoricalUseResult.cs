using System;

namespace CentralBankPublicWebService.DTOs
{
    public class PublicWebServiceHistoricalUseResult
    {
        public DateTime InvocationStart { get; set; }
        public DateTime InvocationEnd { get; set; }
        public string RequestorIp { get; set; }
        public string MethodName { get; set; }
    }
}