using CentralBankPublicWebService.DTOs;
using MySqlConnector;
using System.Collections.Generic;
using System.Configuration;
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
        public List<CreditHistoryResponse> CreditHistory(string juridicTaxpayerIdentificationNumber)
        {
            List<CreditHistoryResponse> creditHistoryResponse = new List<CreditHistoryResponse>();

            using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["default"].ConnectionString))
            {
                connection.Open();

                using (var command = new MySqlCommand("SELECT ENTIDAD.RNC, CONCEPTO_DEUDA.NOMBRE, HIST_CREDITO_CLIENTE.FECHA_CREDITO, HIST_CREDITO_CLIENTE.MONTO " +
                    "FROM HIST_CREDITO_CLIENTE " +
                    "INNER JOIN CLIENTE ON HIST_CREDITO_CLIENTE.CLIENTE_ID = CLIENTE.CLIENTE_ID " +
                    "INNER JOIN CONCEPTO_DEUDA ON HIST_CREDITO_CLIENTE.CONCEPTO_ID = CONCEPTO_DEUDA.CONCEPTO_ID " +
                    "INNER JOIN ENTIDAD ON HIST_CREDITO_CLIENTE.ENTIDAD_ID = ENTIDAD.ENTIDAD_ID " +
                    "WHERE CLIENTE.CEDULA = @juridicTaxpayerIdentificationNumber or CLIENTE.RNC = @juridicTaxpayerIdentificationNumber", connection))
                {
                    command.Parameters.AddWithValue("juridicTaxpayerIdentificationNumber", juridicTaxpayerIdentificationNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            creditHistoryResponse.Add(new CreditHistoryResponse
                            {
                                DebtorJuridicTaxpayerIdentificationNumber = reader.GetString(0),
                                DebtConcept = reader.GetString(1),
                                Date = reader.GetDateTime(2),
                                TotalAmount = reader.GetDecimal(3)
                            });
                        }
                    }
                }
            }

            return creditHistoryResponse;
        }
    }
}
