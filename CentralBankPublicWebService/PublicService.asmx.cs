using CentralBankPublicWebService.Constants;
using CentralBankPublicWebService.DTOs;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Services;

namespace CentralBankPublicWebService
{
    /// <summary>
    /// Summary description for PublicService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class PublicService : WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public List<CreditHistoryResult> CreditHistory(string juridicTaxpayerIdentificationNumber)
        {
            DateTime startOfInvocation = DateTime.UtcNow;
            string requestorIp = HttpContext.Current.Request.UserHostAddress;
            List<CreditHistoryResult> creditHistoryResponse = new List<CreditHistoryResult>();

            using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["default"].ConnectionString))
            {
                connection.Open();

                using (var command = new MySqlCommand("SELECT ENTIDAD.RNC, CONCEPTO_DEUDA.NOMBRE, HIST_CREDITO_CLIENTE.FECHA_CREDITO, HIST_CREDITO_CLIENTE.MONTO " +
                    "FROM HIST_CREDITO_CLIENTE " +
                    "INNER JOIN CLIENTE ON HIST_CREDITO_CLIENTE.CLIENTE_ID = CLIENTE.CLIENTE_ID " +
                    "INNER JOIN CONCEPTO_DEUDA ON HIST_CREDITO_CLIENTE.CONCEPTO_ID = CONCEPTO_DEUDA.CONCEPTO_ID " +
                    "INNER JOIN ENTIDAD ON HIST_CREDITO_CLIENTE.ENTIDAD_ID = ENTIDAD.ENTIDAD_ID " +
                    "WHERE CLIENTE.CEDULA = @juridicTaxpayerIdentificationNumber or CLIENTE.RNC = @juridicTaxpayerIdentificationNumber", 
                    connection))
                {
                    command.Parameters.AddWithValue("juridicTaxpayerIdentificationNumber", juridicTaxpayerIdentificationNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            creditHistoryResponse.Add(new CreditHistoryResult
                            {
                                DebtorJuridicTaxpayerIdentificationNumber = reader.GetString(0),
                                DebtConcept = reader.GetString(1),
                                Date = reader.GetDateTime(2),
                                TotalAmount = reader.GetDecimal(3)
                            });
                        }
                    }
                }
                DateTime endOfInvocation = DateTime.UtcNow;
                using (var command = new MySqlCommand("INSERT INTO CONSULTA_SERVICIO (SERVICIO_ID, FECHA_INVOCACION, FECHA_FINALIZACION, IP_SOLICITANTE) " +
                    "VALUES(@serviceId, @startOfInvocation, @endOfInvocation, @requestorIp)", 
                    connection))
                {
                    command.Parameters.Add("serviceId", MySqlDbType.Int32).Value = WebServiceMethodsId.CreditHistory;
                    command.Parameters.Add("startOfInvocation", MySqlDbType.DateTime).Value = startOfInvocation;
                    command.Parameters.Add("endOfInvocation", MySqlDbType.DateTime).Value = endOfInvocation;
                    command.Parameters.Add("requestorIp", MySqlDbType.VarChar).Value = requestorIp;
                    command.ExecuteNonQuery();
                }
            }

            return creditHistoryResponse;
        }
        
        [WebMethod]
        public CurrencyExchangeResult CurrencyExchange(string currencyCode)
        {
            DateTime startOfInvocation = DateTime.UtcNow;
            string requestorIp = HttpContext.Current.Request.UserHostAddress;
            CurrencyExchangeResult currencyExchangeResult = new CurrencyExchangeResult();

            using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["default"].ConnectionString))
            {
                connection.Open();

                using (var command = new MySqlCommand("SELECT CONVERSION_DOP" +
                    " FROM MONEDA" +
                    " WHERE UPPER(COD_MONEDA) = @currencyCode",
                    connection))
                {
                    command.Parameters.AddWithValue("currencyCode", currencyCode.ToUpper());
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            currencyExchangeResult.DOPConversion = reader.GetDecimal(0);
                        }
                    }
                }
                DateTime endOfInvocation = DateTime.UtcNow;
                using (var command = new MySqlCommand("INSERT INTO CONSULTA_SERVICIO (SERVICIO_ID, FECHA_INVOCACION, FECHA_FINALIZACION, IP_SOLICITANTE) " +
                    "VALUES(@serviceId, @startOfInvocation, @endOfInvocation, @requestorIp)",
                    connection))
                {
                    command.Parameters.Add("serviceId", MySqlDbType.Int32).Value = WebServiceMethodsId.CurrencyExchange;
                    command.Parameters.Add("startOfInvocation", MySqlDbType.DateTime).Value = startOfInvocation;
                    command.Parameters.Add("endOfInvocation", MySqlDbType.DateTime).Value = endOfInvocation;
                    command.Parameters.Add("requestorIp", MySqlDbType.VarChar).Value = requestorIp;
                    command.ExecuteNonQuery();
                }
            }

            return currencyExchangeResult;
        }

        [WebMethod]        
        public FinancialHealthResult FinancialHealth(string juridicTaxpayerIdentificationNumber)
        {
            DateTime startOfInvocation = DateTime.UtcNow;
            string requestorIp = HttpContext.Current.Request.UserHostAddress;
            FinancialHealthResult financialHealthResponse = new FinancialHealthResult();

            using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["default"].ConnectionString))
            {
                connection.Open();

                using (var command = new MySqlCommand("SELECT CONCEPTO_DEUDA.NOMBRE, SUM(HIST_CREDITO_CLIENTE.MONTO) " +
                    "FROM HIST_CREDITO_CLIENTE " +
                    "INNER JOIN CLIENTE ON HIST_CREDITO_CLIENTE.CLIENTE_ID = CLIENTE.CLIENTE_ID " +
                    "INNER JOIN CONCEPTO_DEUDA ON HIST_CREDITO_CLIENTE.CONCEPTO_ID = CONCEPTO_DEUDA.CONCEPTO_ID " +
                    "WHERE CLIENTE.CEDULA = @juridicTaxpayerIdentificationNumber or CLIENTE.RNC = @juridicTaxpayerIdentificationNumber ", connection))
                {
                    command.Parameters.AddWithValue("juridicTaxpayerIdentificationNumber", juridicTaxpayerIdentificationNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            financialHealthResponse.Indicator = reader.GetDecimal(1) > 1000000 ? "N" : "S";
                            financialHealthResponse.Comment = reader.GetString(0);
                            financialHealthResponse.TotalAmount = reader.GetDecimal(1);
                        }
                    }
                }
                DateTime endOfInvocation = DateTime.UtcNow;
                using (var command = new MySqlCommand("INSERT INTO CONSULTA_SERVICIO (SERVICIO_ID, FECHA_INVOCACION, FECHA_FINALIZACION, IP_SOLICITANTE) " +
                    "VALUES(@serviceId, @startOfInvocation, @endOfInvocation, @requestorIp)",
                    connection))
                {
                    command.Parameters.Add("serviceId", MySqlDbType.Int32).Value = WebServiceMethodsId.FinancialHealth;
                    command.Parameters.Add("startOfInvocation", MySqlDbType.DateTime).Value = startOfInvocation;
                    command.Parameters.Add("endOfInvocation", MySqlDbType.DateTime).Value = endOfInvocation;
                    command.Parameters.Add("requestorIp", MySqlDbType.VarChar).Value = requestorIp;
                    command.ExecuteNonQuery();
                }
            }
            return financialHealthResponse;
        }

        [WebMethod]
        public InflationRateResult InflationRate(string period)
        {
            DateTime startOfInvocation = DateTime.UtcNow;
            string requestorIp = HttpContext.Current.Request.UserHostAddress;
            InflationRateResult inflationRateResult = new InflationRateResult();

            using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["default"].ConnectionString))
            {
                connection.Open();

                using (var command = new MySqlCommand("SELECT HIST_INFLACION_MONEDA.RATE " +
                    "FROM HIST_INFLACION_MONEDA " +
                    "WHERE DATE_FORMAT(HIST_INFLACION_MONEDA.PERIODO, '%Y%m') = @period", connection))
                {
                    command.Parameters.AddWithValue("period", period);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            inflationRateResult.Rate = reader.GetDecimal(0);
                        }
                    }
                }
                DateTime endOfInvocation = DateTime.UtcNow;
                using (var command = new MySqlCommand("INSERT INTO CONSULTA_SERVICIO (SERVICIO_ID, FECHA_INVOCACION, FECHA_FINALIZACION, IP_SOLICITANTE) " +
                    "VALUES(@serviceId, @startOfInvocation, @endOfInvocation, @requestorIp)",
                    connection))
                {
                    command.Parameters.Add("serviceId", MySqlDbType.Int32).Value = WebServiceMethodsId.InflationRate;
                    command.Parameters.Add("startOfInvocation", MySqlDbType.DateTime).Value = startOfInvocation;
                    command.Parameters.Add("endOfInvocation", MySqlDbType.DateTime).Value = endOfInvocation;
                    command.Parameters.Add("requestorIp", MySqlDbType.VarChar).Value = requestorIp;
                    command.ExecuteNonQuery();
                }
            }
            return inflationRateResult;
        }
    }
}
