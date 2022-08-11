using CentralBankPublicWebService.Constants;
using CentralBankPublicWebService.DTOs;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Services;

namespace CentralBankPublicWebService
{
    /// <summary>
    /// Summary description for PrivateService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class PrivateService : WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public List<PublicWebServiceHistoricalUseResult> PublicWebServiceHistoricalUse(string password, string methodName, DateTime? start, DateTime? end)
        {
            methodName = methodName == string.Empty ? null : methodName;
            DateTime startOfInvocation = DateTime.UtcNow;
            string requestorIp = HttpContext.Current.Request.UserHostAddress;
            List<PublicWebServiceHistoricalUseResult> publicWebServiceHistoricalUseResult = new List<PublicWebServiceHistoricalUseResult>();

            if (ConfigurationManager.AppSettings["PrivateServicePassword"] != password)
            {
                Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return publicWebServiceHistoricalUseResult;
            }
                

            using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["default"].ConnectionString))
            {
                connection.Open();
                string query = "SELECT CONSULTA_SERVICIO.FECHA_INVOCACION, CONSULTA_SERVICIO.FECHA_FINALIZACION, CONSULTA_SERVICIO.IP_SOLICITANTE, SERVICIO.NOMBRE " +
                    "FROM CONSULTA_SERVICIO " +
                    "INNER JOIN SERVICIO ON CONSULTA_SERVICIO.SERVICIO_ID = SERVICIO.SERVICIO_ID " +
                    "WHERE (@methodName IS NULL OR SERVICIO.NOMBRE = @methodName) " +
                    "AND (@start IS NULL OR CONSULTA_SERVICIO.FECHA_INVOCACION >= @start) " +
                    "AND (@end IS NULL OR CONSULTA_SERVICIO.FECHA_FINALIZACION <= @end)";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("methodName", methodName);
                    command.Parameters.AddWithValue("start", start);
                    command.Parameters.AddWithValue("end", end);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            publicWebServiceHistoricalUseResult.Add(new PublicWebServiceHistoricalUseResult
                            {
                                InvocationStart = reader.GetDateTime(0),
                                InvocationEnd = reader.GetDateTime(1),
                                RequestorIp = reader.GetString(2),
                                MethodName = reader.GetString(3),
                            });
                        }
                    }
                }
                DateTime endOfInvocation = DateTime.UtcNow;
                using (var command = new MySqlCommand("INSERT INTO CONSULTA_SERVICIO (SERVICIO_ID, FECHA_INVOCACION, FECHA_FINALIZACION, IP_SOLICITANTE) " +
                    "VALUES(@serviceId, @startOfInvocation, @endOfInvocation, @requestorIp)",
                    connection))
                {
                    command.Parameters.Add("serviceId", MySqlDbType.Int32).Value = WebServiceMethodsId.PublicWebServiceHistoricalUseResult;
                    command.Parameters.Add("startOfInvocation", MySqlDbType.DateTime).Value = startOfInvocation;
                    command.Parameters.Add("endOfInvocation", MySqlDbType.DateTime).Value = endOfInvocation;
                    command.Parameters.Add("requestorIp", MySqlDbType.VarChar).Value = requestorIp;
                    command.ExecuteNonQuery();
                }
            }

            return publicWebServiceHistoricalUseResult;
        }
    }
}
